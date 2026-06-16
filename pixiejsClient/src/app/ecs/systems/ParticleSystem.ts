import { System2 } from "../core/System";
import { NTT } from "../core/NTT";
import { World } from "../core/World";
import { ParticleSystemComponent } from "../components/ParticleSystemComponent";
import { PhysicsComponent } from "../components/PhysicsComponent";
import { ParentChildComponent } from "../components/ParentChildComponent";
import { EngineComponent } from "../components/EngineComponent";

export class ParticleSystem extends System2<ParticleSystemComponent, PhysicsComponent> {
  private inputManager: any;

  constructor(inputManager: any) {
    super(ParticleSystemComponent, PhysicsComponent);
    this.inputManager = inputManager;
  }

  protected updateEntity(ntt: NTT, psc: ParticleSystemComponent, phy: PhysicsComponent, deltaTime: number): void {
    psc.update(deltaTime);

    if (ntt !== World.Me) return;

    const inputState = this.inputManager.getInputState();
    const isEngineActive = inputState.thrust || inputState.left || inputState.right;

    if (!isEngineActive) return;

    const gridSize = 1.0;
    const entityRotation = phy.rotationRadians;

    const allEntities = World.getAllEntities();
    for (const childEntity of allEntities) {
      const parentChild = childEntity.get(ParentChildComponent);
      if (!parentChild || parentChild.parentId !== ntt.id) continue;

      const engine = childEntity.get(EngineComponent);
      if (!engine) continue;

      const offsetX = parentChild.gridX * gridSize;
      const offsetY = parentChild.gridY * gridSize;

      const shouldFire = this.shouldEngineEmitParticles(inputState, offsetY);
      if (!shouldFire) continue;

      const cos = Math.cos(entityRotation);
      const sin = Math.sin(entityRotation);
      const rotatedOffsetX = offsetX * cos - offsetY * sin;
      const rotatedOffsetY = offsetX * sin + offsetY * cos;

      const engineWorldX = phy.position.x + rotatedOffsetX;
      const engineWorldY = phy.position.y + rotatedOffsetY;

      const partRotationRadians = parentChild.rotation * (Math.PI / 2);
      const engineRotation = partRotationRadians + entityRotation;

      const exhaustDirection = engineRotation + Math.PI;

      const nozzleDistance = gridSize * 0.6;
      const emissionX = engineWorldX + Math.cos(exhaustDirection) * nozzleDistance;
      const emissionY = engineWorldY + Math.sin(exhaustDirection) * nozzleDistance;

      const intensity = inputState.thrust ? 1.0 : 0.7;
      psc.emitParticles(emissionX, emissionY, exhaustDirection, intensity);
    }
  }

  private shouldEngineEmitParticles(inputState: any, offsetY: number): boolean {
    if (inputState.thrust || inputState.boost) return true;
    else if (inputState.left || inputState.right) return (inputState.left && offsetY > 0) || (inputState.right && offsetY < 0);
    return false;
  }
}
