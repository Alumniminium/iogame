/**
 * Component registry index - imports all components to ensure decorators run
 * This file is imported early to guarantee all components are registered
 */

// Import all components - the decorators will auto-register them
export { BodyDamageComponent } from "./BodyDamageComponent";
export { PhysicsComponent as PhysicsComponent } from "./PhysicsComponent";
export { BulletComponent } from "./BulletComponent";
export { ColorComponent } from "./ColorComponent";
export { DamageComponent } from "./DamageComponent";
export { DeathTagComponent } from "./DeathTagComponent";
export { EnergyComponent } from "./EnergyComponent";
export { EngineComponent } from "./EngineComponent";
export { ExpRewardComponent } from "./ExpRewardComponent";
export { GravityComponent } from "./GravityComponent";
export { HealthComponent } from "./HealthComponent";
export { HealthRegenComponent } from "./HealthRegenComponent";
export { InputComponent } from "./InputComponent";
export { InventoryComponent } from "./InventoryComponent";
export { LevelComponent } from "./LevelComponent";
export { LifeTimeComponent } from "./LifeTimeComponent";
export { NetworkComponent } from "./NetworkComponent";
export { ParentChildComponent } from "./ParentChildComponent";
export { PickableTagComponent } from "./PickableTagComponent";
export { RenderComponent } from "./RenderComponent";
export { RespawnTagComponent } from "./RespawnTagComponent";
export { ShieldComponent } from "./ShieldComponent";
export { ShipPartComponent } from "./ShipPartComponent";
export { SpawnerComponent } from "./SpawnerComponent";
export { WeaponComponent } from "./WeaponComponent";

// Export the registry for debugging
export { ComponentRegistry } from "../core/Component";
