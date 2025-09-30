import { System } from "../core/System";
import { Entity } from "../core/Entity";
import { ParticleSystemComponent } from "../components/ParticleSystemComponent";
import { PhysicsComponent } from "../components/PhysicsComponent";
import { RenderComponent } from "../components/RenderComponent";
// import { NetworkComponent } from "../components/NetworkComponent";

/**
 * Manages particle emission and lifecycle for visual effects.
 * Primarily used for engine exhaust particles on locally-controlled entities.
 */
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

    particleSystem.update(deltaTime);

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

    for (const part of render.shipParts) {
      if (part.type !== 2) continue;

      const offsetX = part.gridX * gridSize;
      const offsetY = part.gridY * gridSize;

      const shouldFire = this.shouldEngineEmitParticles(inputState, offsetY);
      if (!shouldFire) continue;

      const cos = Math.cos(entityRotation);
      const sin = Math.sin(entityRotation);
      const rotatedOffsetX = offsetX * cos - offsetY * sin;
      const rotatedOffsetY = offsetX * sin + offsetY * cos;

      const engineWorldX = physics.position.x + rotatedOffsetX;
      const engineWorldY = physics.position.y + rotatedOffsetY;

      const partRotationRadians = part.rotation * (Math.PI / 2);
      const engineRotation = partRotationRadians + entityRotation;

      const exhaustDirection = engineRotation + Math.PI;

      const nozzleDistance = gridSize * 0.6;
      const emissionX =
        engineWorldX + Math.cos(exhaustDirection) * nozzleDistance;
      const emissionY =
        engineWorldY + Math.sin(exhaustDirection) * nozzleDistance;

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
    if (inputState.thrust || inputState.boost) return true;
    else if (inputState.left || inputState.right)
      return (
        (inputState.left && offsetY > 0) || (inputState.right && offsetY < 0)
      );
    return false;
  }
}
