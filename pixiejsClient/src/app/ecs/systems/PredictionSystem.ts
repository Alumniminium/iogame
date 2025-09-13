import { System } from "../core/System";
import { Entity } from "../core/Entity";
import { PhysicsComponent } from "../components/PhysicsComponent";
import { NetworkComponent } from "../components/NetworkComponent";
import { InputBuffer, InputSnapshot } from "./InputBuffer";
import { Vector2 } from "../core/types";
import { TickSynchronizer } from "../../network/TickSynchronizer";

export class PredictionSystem extends System {
  readonly componentTypes = [PhysicsComponent, NetworkComponent];

  private inputBuffer = new InputBuffer();
  private localEntityId: number | null = null;
  private tickSynchronizer = TickSynchronizer.getInstance();

  setLocalEntity(entityId: number): void {
    this.localEntityId = entityId;
  }

  addInputSnapshot(snapshot: InputSnapshot): void {
    this.inputBuffer.addInput(snapshot);
  }

  protected updateEntity(entity: Entity, deltaTime: number): void {
    const physics = entity.get(PhysicsComponent)!;
    const network = entity.get(NetworkComponent)!;

    if (network.isLocallyControlled && entity.id === this.localEntityId) {
      // Handle local player with prediction
      this.updateLocalPlayer(entity, physics, network, deltaTime);
    } else {
      // Handle remote players with interpolation
      this.updateRemotePlayer(entity, physics, network, deltaTime);
    }
  }

  private updateLocalPlayer(
    entity: Entity,
    physics: PhysicsComponent,
    network: NetworkComponent,
    deltaTime: number
  ): void {
    // Only reconcile if we've received at least one server update
    // and we're marked for reconciliation
    if (network.needsReconciliation && network.lastInputSequence > 0) {
      // Check for deceleration drift before reconciliation
      this.checkForDecelerationDrift(physics, network);

      this.performReconciliation(physics, network);
      network.clearReconciliation();

      // CRITICAL: Update predicted state to match corrected state
      // This ensures future predictions build from the corrected position
      network.updatePredictedState(
        physics.position,
        physics.linearVelocity,
        physics.rotationRadians
      );
    } else {
      // Normal prediction: store current state as predicted state
      network.updatePredictedState(
        physics.position,
        physics.linearVelocity,
        physics.rotationRadians
      );
    }
  }

  private updateRemotePlayer(
    entity: Entity,
    physics: PhysicsComponent,
    network: NetworkComponent,
    deltaTime: number
  ): void {
    // Interpolate toward server position for remote players
    const lerpFactor = Math.min(deltaTime * 8, 1); // Smooth interpolation

    // Lerp position toward server position
    physics.position.x += (network.serverPosition.x - physics.position.x) * lerpFactor;
    physics.position.y += (network.serverPosition.y - physics.position.y) * lerpFactor;

    // Lerp rotation toward server rotation
    let rotationDiff = network.serverRotation - physics.rotationRadians;

    // Handle rotation wrapping
    if (rotationDiff > Math.PI) rotationDiff -= 2 * Math.PI;
    if (rotationDiff < -Math.PI) rotationDiff += 2 * Math.PI;

    physics.rotationRadians += rotationDiff * lerpFactor;

    // Update velocity toward server velocity
    physics.linearVelocity.x += (network.serverVelocity.x - physics.linearVelocity.x) * lerpFactor;
    physics.linearVelocity.y += (network.serverVelocity.y - physics.linearVelocity.y) * lerpFactor;

    physics.markChanged();
  }

  private performReconciliation(physics: PhysicsComponent, network: NetworkComponent): void {
    // CRITICAL FIX: Only compare positions if we have proper tick synchronization
    if (!network.serverTick || !this.tickSynchronizer.isSynchronized()) {
      // Fallback to time-based reconciliation if no tick data available
      this.performTimeBasedReconciliation(physics, network);
      return;
    }

    // Calculate the client's predicted position at the same tick as the server state
    const currentServerTick = this.tickSynchronizer.getCurrentServerTick();
    const tickDifference = currentServerTick - network.serverTick;

    // If server state is too old (more than 5 ticks), skip reconciliation to avoid false positives
    if (tickDifference > 5) {
      return;
    }

    // Calculate velocity and position differences for collision detection
    const velDiff = Math.sqrt(
      Math.pow(network.serverVelocity.x - physics.linearVelocity.x, 2) +
      Math.pow(network.serverVelocity.y - physics.linearVelocity.y, 2)
    );
    const positionDiff = this.calculateDistance(physics.position, network.serverPosition);

    // Early collision detection - if velocity difference is huge, likely collision occurred
    if (velDiff > 50) {
      console.log(`Large velocity discrepancy detected (${velDiff.toFixed(1)}) - likely collision, forcing server reconciliation`);
      // Skip normal reconciliation logic, go straight to server state
      physics.position = { ...network.serverPosition };
      physics.linearVelocity = { ...network.serverVelocity };
      physics.rotationRadians = network.serverRotation;
      physics.angularVelocity = 0;
      physics.acceleration = { x: 0, y: 0 };
      physics.markChanged();
      return;
    }

    // Get the client's state at the server tick by rewinding the input buffer
    const clientStateAtServerTick = this.getClientStateAtTick(physics, network, network.serverTick);
    if (!clientStateAtServerTick) {
      // Fallback if we don't have the historical state
      this.performTimeBasedReconciliation(physics, network);
      return;
    }

    // Now compare positions at the same tick
    const tickBasedPositionDiff = this.calculateDistance(clientStateAtServerTick.position, network.serverPosition);

    if (tickBasedPositionDiff > network.reconciliationThreshold) {
      // Use hybrid approach: input replay for large discrepancies, lerp for small ones
      const largeDiscrepancyThreshold = network.reconciliationThreshold * 3; // 15 pixels by default

      if (tickBasedPositionDiff > largeDiscrepancyThreshold) {
        // Large discrepancy: Use proper input replay reconciliation
        this.performInputReplayReconciliation(physics, network);
      } else {
        // Small discrepancy: Use smooth lerp correction
        this.performLerpReconciliation(physics, network);
      }

      // Additional aggressive velocity reset if we detect significant drift after deceleration
      if (velDiff > 20 && Math.sqrt(physics.linearVelocity.x ** 2 + physics.linearVelocity.y ** 2) < 50) {
        // If velocity difference is large but current velocity is low (deceleration scenario)
        // Force reset to server velocity to prevent jumping
        physics.linearVelocity = { ...network.serverVelocity };
      }

      physics.markChanged();
    }
  }

  private performInputReplayReconciliation(physics: PhysicsComponent, network: NetworkComponent): void {
    // Find the input that corresponds to the server state
    const serverSequence = network.lastInputSequence;
    const inputSnapshot = this.inputBuffer.getInput(serverSequence);

    if (inputSnapshot) {
      // Check if collision likely occurred by detecting large velocity changes
      const velDiff = Math.sqrt(
        Math.pow(network.serverVelocity.x - physics.linearVelocity.x, 2) +
        Math.pow(network.serverVelocity.y - physics.linearVelocity.y, 2)
      );

      const positionDiff = this.calculateDistance(physics.position, network.serverPosition);

      // If there's a large velocity difference AND significant position error, likely collision occurred
      const collisionSuspected = velDiff > 30 || positionDiff > 20;

      if (collisionSuspected) {
        // For suspected collisions, don't do input replay - just snap to server state
        // Collision physics is too complex to predict accurately on client
        physics.position = { ...network.serverPosition };
        physics.linearVelocity = { ...network.serverVelocity };
        physics.rotationRadians = network.serverRotation;
        physics.angularVelocity = 0;
        physics.acceleration = { x: 0, y: 0 };

        console.log(`Collision suspected (velDiff: ${velDiff.toFixed(1)}, posDiff: ${positionDiff.toFixed(1)}) - snapping to server state`);
      } else {
        // Normal input replay for non-collision scenarios
        physics.position = { ...network.serverPosition };
        if (velDiff > 5) { // Much lower threshold - reconcile for differences > 5 units/sec
          physics.linearVelocity = { ...network.serverVelocity };
        }
        physics.rotationRadians = network.serverRotation;
        physics.angularVelocity = 0; // Reset angular velocity
        physics.acceleration = { x: 0, y: 0 }; // Clear any pending forces

        // Replay all inputs after the server sequence
        const inputsToReplay = this.inputBuffer.getInputsAfter(serverSequence);

        for (const input of inputsToReplay) {
          this.simulatePhysicsForInput(physics, input, 1 / 60); // Server runs at 60 TPS
        }
      }

      // Clean up old inputs
      this.inputBuffer.removeInputsBefore(serverSequence - 120); // Keep 2 seconds of history at 60hz
    } else {
      this.performLerpReconciliation(physics, network);
    }
  }

  private performLerpReconciliation(physics: PhysicsComponent, network: NetworkComponent): void {
    // Use more aggressive lerp factor for larger threshold (50px)
    const positionDiff = this.calculateDistance(physics.position, network.serverPosition);
    const lerpFactor = Math.min(0.3 + (positionDiff / 100), 0.8); // Scale lerp factor based on error size

    // Lerp position toward server position
    physics.position.x += (network.serverPosition.x - physics.position.x) * lerpFactor;
    physics.position.y += (network.serverPosition.y - physics.position.y) * lerpFactor;

    // Be more aggressive with velocity reconciliation to prevent drift
    const velDiff = Math.sqrt(
      Math.pow(network.serverVelocity.x - physics.linearVelocity.x, 2) +
      Math.pow(network.serverVelocity.y - physics.linearVelocity.y, 2)
    );

    if (velDiff > 10) { // Lower threshold - adjust for differences > 10 units/sec
      const velLerpFactor = lerpFactor * 0.6; // More aggressive velocity lerp
      physics.linearVelocity.x += (network.serverVelocity.x - physics.linearVelocity.x) * velLerpFactor;
      physics.linearVelocity.y += (network.serverVelocity.y - physics.linearVelocity.y) * velLerpFactor;
    } else if (velDiff > 2) { // Even small differences should be corrected gently
      const velLerpFactor = 0.1; // Gentle correction for small differences
      physics.linearVelocity.x += (network.serverVelocity.x - physics.linearVelocity.x) * velLerpFactor;
      physics.linearVelocity.y += (network.serverVelocity.y - physics.linearVelocity.y) * velLerpFactor;
    }

    // Lerp rotation toward server rotation
    let rotationDiff = network.serverRotation - physics.rotationRadians;

    // Handle rotation wrapping
    if (rotationDiff > Math.PI) rotationDiff -= 2 * Math.PI;
    if (rotationDiff < -Math.PI) rotationDiff += 2 * Math.PI;

    physics.rotationRadians += rotationDiff * lerpFactor;

    // Only clear forces if we're making significant corrections
    if (positionDiff > 20) {
      physics.acceleration = { x: 0, y: 0 };
      physics.angularVelocity = 0;
    }
  }

  private simulatePhysicsStep(state: any, input: InputSnapshot, deltaTime: number): void {
    const thrustForce = 500; // Match InputSystem constants
    const rotationSpeed = 3;

    // Apply torque from input
    if (input.inputState.left) {
      state.angularVelocity -= rotationSpeed * deltaTime;
    }
    if (input.inputState.right) {
      state.angularVelocity += rotationSpeed * deltaTime;
    }

    // Apply thrust from input
    if (input.inputState.thrust) {
      const forceX = Math.cos(state.rotation - Math.PI / 2) * thrustForce;
      const forceY = Math.sin(state.rotation - Math.PI / 2) * thrustForce;
      state.acceleration.x += forceX;
      state.acceleration.y += forceY;
    }

    if (input.inputState.invThrust) {
      const forceX = Math.cos(state.rotation + Math.PI / 2) * thrustForce * 0.5;
      const forceY = Math.sin(state.rotation + Math.PI / 2) * thrustForce * 0.5;
      state.acceleration.x += forceX;
      state.acceleration.y += forceY;
    }

    // Apply basic physics integration
    state.velocity.x += state.acceleration.x * deltaTime;
    state.velocity.y += state.acceleration.y * deltaTime;
    state.acceleration.x = 0;
    state.acceleration.y = 0;

    state.position.x += state.velocity.x * deltaTime;
    state.position.y += state.velocity.y * deltaTime;
    state.rotation += state.angularVelocity * deltaTime;

    // Apply drag (assuming 0.02 drag factor like in PhysicsSystem)
    const dragFactor = 1 - 0.02;
    state.velocity.x *= dragFactor;
    state.velocity.y *= dragFactor;
    state.angularVelocity *= dragFactor;
  }

  private simulatePhysicsForInput(physics: PhysicsComponent, input: InputSnapshot, deltaTime: number): void {
    const thrustForce = 500; // Match InputSystem constants
    const rotationSpeed = 3;

    // Apply torque from input
    if (input.inputState.left) {
      physics.angularVelocity -= rotationSpeed * deltaTime;
    }
    if (input.inputState.right) {
      physics.angularVelocity += rotationSpeed * deltaTime;
    }

    // Apply thrust from input
    if (input.inputState.thrust) {
      const forceX = Math.cos(physics.rotationRadians - Math.PI / 2) * thrustForce;
      const forceY = Math.sin(physics.rotationRadians - Math.PI / 2) * thrustForce;
      physics.addForce({ x: forceX, y: forceY });
    }

    if (input.inputState.invThrust) {
      const forceX = Math.cos(physics.rotationRadians + Math.PI / 2) * thrustForce * 0.5;
      const forceY = Math.sin(physics.rotationRadians + Math.PI / 2) * thrustForce * 0.5;
      physics.addForce({ x: forceX, y: forceY });
    }

    // Apply basic physics integration (simplified version of PhysicsSystem)
    physics.linearVelocity.x += physics.acceleration.x * deltaTime;
    physics.linearVelocity.y += physics.acceleration.y * deltaTime;
    physics.acceleration.x = 0;
    physics.acceleration.y = 0;

    physics.position.x += physics.linearVelocity.x * deltaTime;
    physics.position.y += physics.linearVelocity.y * deltaTime;
    physics.rotationRadians += physics.angularVelocity * deltaTime;

    // Apply drag - use consistent drag factor with server (0.02)
    const dragFactor = 1 - 0.02; // Match server drag exactly
    physics.linearVelocity.x *= dragFactor;
    physics.linearVelocity.y *= dragFactor;
    physics.angularVelocity *= dragFactor;
  }

  private calculateDistance(pos1: Vector2, pos2: Vector2): number {
    const dx = pos1.x - pos2.x;
    const dy = pos1.y - pos2.y;
    return Math.sqrt(dx * dx + dy * dy);
  }

  private getClientStateAtTick(physics: PhysicsComponent, network: NetworkComponent, targetTick: number): { position: Vector2, velocity: Vector2, rotation: number } | null {
    // IMPROVED: Calculate the client's position at the target tick by replaying from server state
    const serverSequence = network.lastInputSequence;

    // If we don't have the input for the target tick, we can't calculate it accurately
    const targetInput = this.inputBuffer.getInput(targetTick);
    if (!targetInput) {
      return null;
    }

    // Start from the last confirmed server state
    const simulatedState = {
      position: { ...network.serverPosition },
      velocity: { ...network.serverVelocity },
      rotation: network.serverRotation,
      angularVelocity: 0,
      acceleration: { x: 0, y: 0 }
    };

    // Replay inputs from server sequence up to target tick
    const inputsToReplay = this.inputBuffer.getInputsInRange(serverSequence, targetTick);

    for (const input of inputsToReplay) {
      this.simulatePhysicsStep(simulatedState, input, 1 / 60);
    }

    return {
      position: simulatedState.position,
      velocity: simulatedState.velocity,
      rotation: simulatedState.rotation
    };
  }

  private performTimeBasedReconciliation(physics: PhysicsComponent, network: NetworkComponent): void {
    // Fallback to the original time-based reconciliation logic
    const positionDiff = this.calculateDistance(physics.position, network.serverPosition);

    if (positionDiff > network.reconciliationThreshold) {
      const largeDiscrepancyThreshold = network.reconciliationThreshold * 3;

      if (positionDiff > largeDiscrepancyThreshold) {
        this.performInputReplayReconciliation(physics, network);
      } else {
        this.performLerpReconciliation(physics, network);
      }

      physics.markChanged();
    }
  }

  private checkForDecelerationDrift(physics: PhysicsComponent, network: NetworkComponent): void {
    // Detect the specific case of deceleration drift after high-speed movement
    const clientSpeed = Math.sqrt(physics.linearVelocity.x ** 2 + physics.linearVelocity.y ** 2);
    const serverSpeed = Math.sqrt(network.serverVelocity.x ** 2 + network.serverVelocity.y ** 2);

    const velDiff = Math.sqrt(
      Math.pow(network.serverVelocity.x - physics.linearVelocity.x, 2) +
      Math.pow(network.serverVelocity.y - physics.linearVelocity.y, 2)
    );

    // If both speeds are low but there's still a velocity difference, likely deceleration drift
    if (clientSpeed < 30 && serverSpeed < 30 && velDiff > 5) {
      // Aggressively reset to server velocity to prevent jumping
      physics.linearVelocity = { ...network.serverVelocity };
      physics.acceleration = { x: 0, y: 0 }; // Clear any pending forces
    }

    // Also handle case where server has stopped but client is still moving slowly
    if (serverSpeed < 5 && clientSpeed > 10 && velDiff > 8) {
      // Server has essentially stopped, force client to match
      physics.linearVelocity = { ...network.serverVelocity };
      physics.acceleration = { x: 0, y: 0 };
    }
  }

  onServerUpdate(entityId: number, position: Vector2, velocity: Vector2, rotation: number, inputSequence: number, serverTick?: number): void {
    if (entityId === this.localEntityId) {
      const entity = this.getEntity(entityId);
      if (entity) {
        const network = entity.get(NetworkComponent);
        if (network) {
          // Store the server tick timestamp if provided, otherwise use current time
          const timestamp = serverTick !== undefined ? serverTick : Date.now();
          network.updateServerState(position, velocity, rotation, timestamp);
          network.lastInputSequence = inputSequence;
          network.serverTick = serverTick; // Store server tick for proper temporal comparison
          network.markForReconciliation();
        }
      }
    }
  }
}