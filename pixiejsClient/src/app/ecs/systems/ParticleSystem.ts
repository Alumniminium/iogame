import { System } from "../core/System";
import { Entity } from "../core/Entity";
import { ParticleSystemComponent } from "../components/ParticleSystemComponent";
import { PhysicsComponent } from "../components/PhysicsComponent";
import { RenderComponent } from "../components/RenderComponent";
import { EngineComponent } from "../components/EngineComponent";
import { NetworkComponent } from "../components/NetworkComponent";

export class ParticleSystem extends System {
  readonly componentTypes = [
    ParticleSystemComponent,
    PhysicsComponent,
    RenderComponent,
  ];

  private inputManager: any = null;
  private localPlayerId: string | null = null;

  setInputManager(inputManager: any, localPlayerId: string): void {
    this.inputManager = inputManager;
    this.localPlayerId = localPlayerId;
  }

  protected updateEntity(entity: Entity, deltaTime: number): void {
    const particleSystem = entity.get(ParticleSystemComponent)!;
    const physics = entity.get(PhysicsComponent)!;
    const render = entity.get(RenderComponent)!;
    const engine = entity.get(EngineComponent);

    // Update existing particles
    particleSystem.update(deltaTime);

    // Only emit particles for local player when engines are active
    if (
      render.shipParts &&
      entity.id === this.localPlayerId &&
      this.inputManager
    ) {
      const inputState = this.inputManager.getInputState();
      const isEngineActive =
        inputState.thrust || inputState.left || inputState.right;

      if (isEngineActive) {
        this.emitEngineParticles(particleSystem, render, physics, inputState);
      }
    }
  }

  private emitEngineParticles(
    particleSystem: ParticleSystemComponent,
    render: RenderComponent,
    physics: PhysicsComponent,
    inputState: any,
  ): void {
    const gridSize = 1.0;
    const entityRotation = physics.rotationRadians;

    // Find all engine parts and emit particles based on which engines should fire
    for (const part of render.shipParts) {
      if (part.type !== 2) continue; // Only engine parts

      const offsetX = part.gridX * gridSize; // Part coordinates are already centered
      const offsetY = part.gridY * gridSize; // Part coordinates are already centered

      // Determine if this engine should fire based on input and position
      const shouldFire = this.shouldEngineEmitParticles(inputState, offsetY);
      if (!shouldFire) continue;

      // Calculate engine part position in world coordinates
      const cos = Math.cos(entityRotation);
      const sin = Math.sin(entityRotation);
      const rotatedOffsetX = offsetX * cos - offsetY * sin;
      const rotatedOffsetY = offsetX * sin + offsetY * cos;

      const engineWorldX = physics.position.x + rotatedOffsetX;
      const engineWorldY = physics.position.y + rotatedOffsetY;

      // Calculate engine part rotation (part rotation + entity rotation)
      const partRotationRadians = part.rotation * (Math.PI / 2);
      const engineRotation = partRotationRadians + entityRotation;

      // Exhaust comes out the back of the engine (add 180 degrees)
      const exhaustDirection = engineRotation + Math.PI;

      // Position particles slightly behind the engine nozzle
      const nozzleDistance = gridSize * 0.6;
      const emissionX =
        engineWorldX + Math.cos(exhaustDirection) * nozzleDistance;
      const emissionY =
        engineWorldY + Math.sin(exhaustDirection) * nozzleDistance;

      // Emit particles with intensity based on input
      const intensity = inputState.thrust ? 1.0 : 0.7;
      particleSystem.emitParticles(
        emissionX,
        emissionY,
        exhaustDirection,
        intensity,
      );
    }
  }

  private shouldEngineEmitParticles(inputState: any, offsetY: number): boolean {
    // Mirror the server-side engine firing logic
    if (inputState.thrust || inputState.boost) {
      return true; // All engines fire for thrust/boost
    } else if (inputState.left || inputState.right) {
      // Only specific engines fire for rotation
      return (
        (inputState.left && offsetY > 0) || (inputState.right && offsetY < 0)
      );
    }
    return false;
  }
}
