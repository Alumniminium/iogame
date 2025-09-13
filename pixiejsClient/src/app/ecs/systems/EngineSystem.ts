import { System } from "../core/System";
import { Entity } from "../core/Entity";
import { PhysicsComponent } from "../components/PhysicsComponent";
import { EngineComponent } from "../components/EngineComponent";
import { EnergyComponent } from "../components/EnergyComponent";

export class EngineSystem extends System {
  readonly componentTypes = [PhysicsComponent, EngineComponent, EnergyComponent];

  protected updateEntity(entity: Entity, deltaTime: number): void {
    const physics = entity.get(PhysicsComponent);
    const engine = entity.get(EngineComponent);
    const energy = entity.get(EnergyComponent);

    if (!physics || !engine || !energy) return;

    // Match server logic exactly from EngineSystem.cs
    const powerDraw = engine.powerUse * engine.throttle;

    // Check energy constraints
    if (energy.current < powerDraw) {
      engine.throttle = energy.current / engine.powerUse;
      const adjustedPowerDraw = engine.powerUse * engine.throttle;
      // In client-side prediction, we don't track energy discharge
      // That's handled by server authority
    }

    // Calculate propulsion force
    const forward = physics.forward;
    const propulsionForce = engine.maxPropulsion * engine.throttle;
    const propulsion = {
      x: forward.x * propulsionForce,
      y: forward.y * propulsionForce
    };

    // Early exit if no forces to apply
    if (propulsion.x === 0 && propulsion.y === 0 && engine.rotation === 0 && !engine.rcs) {
      return;
    }

    // Set drag based on RCS state
    physics.drag = engine.rcs ? 0.01 : 0.001;

    // Apply propulsion force (matching server: acceleration is added directly to velocity per tick)
    physics.acceleration.x += propulsion.x * deltaTime;
    physics.acceleration.y += propulsion.y * deltaTime;

    // Apply angular velocity directly (matches server line 32: phy.AngularVelocity = eng.Rotation * 3)
    physics.angularVelocity = engine.rotation * 3;

    physics.markChanged();
  }
}