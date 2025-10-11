/**
 * Component registry index - imports all components to ensure decorators run
 * This file is imported early to guarantee all components are registered
 */

// Import all components - the decorators will auto-register them
export { Box2DBodyComponent } from "./Box2DBodyComponent";
export { ColorComponent } from "./ColorComponent";
export { EnergyComponent } from "./EnergyComponent";
export { EngineComponent } from "./EngineComponent";
export { GravityComponent } from "./GravityComponent";
export { HealthComponent } from "./HealthComponent";
export { InputComponent } from "./InputComponent";
export { LifeTimeComponent } from "./LifeTimeComponent";
export { NetworkComponent } from "./NetworkComponent";
export { ParentChildComponent } from "./ParentChildComponent";
export { RenderComponent } from "./RenderComponent";
export { ShieldComponent } from "./ShieldComponent";
export { ShipPartComponent } from "./ShipPartComponent";

// Export the registry for debugging
export { ComponentRegistry } from "../core/Component";
