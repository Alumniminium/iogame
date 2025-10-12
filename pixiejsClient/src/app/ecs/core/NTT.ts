import { Component } from "./Component";
import { World } from "./World";

/**
 * Entity represents a game object in the ECS architecture.
 * Entities are containers for components and are identified by unique IDs.
 */
export class NTT {
  readonly id: string;
  private components = new Map<string, Component>();

  constructor(id: string) {
    this.id = id;
  }

  static from(id: string): NTT {
    return World.getEntity(id) ?? World.createEntity(id);
  }

  /**
   * Adds or updates a component on this entity.
   * Notifies the World of the component change.
   */
  set<T extends Component>(component: T): void {
    const key = component.constructor.name;
    this.components.set(key, component);
    World.informChangesFor(this);
  }

  /**
   * Retrieves a component of the specified type from this entity.
   * @returns The component instance, or undefined if not found
   */
  get<T extends Component>(componentClass: new (ntt: NTT, ...args: any[]) => T): T | undefined {
    return this.components.get(componentClass.name) as T;
  }

  /**
   * Checks if this entity has a component of the specified type.
   */
  has<T extends Component>(componentClass: new (ntt: NTT, ...args: any[]) => T): boolean {
    return this.components.has(componentClass.name);
  }

  /**
   * Checks if this entity has all of the specified component types.
   */
  hasAll<T extends Component>(...componentClasses: (new (ntt: NTT, ...args: any[]) => T)[]): boolean {
    return componentClasses.every((compClass) => this.has(compClass));
  }

  /**
   * Removes a component of the specified type from this entity.
   * Notifies the World if the component was successfully removed.
   */
  remove<T extends Component>(componentClass: new (ntt: NTT, ...args: any[]) => T): void {
    const key = componentClass.name;
    const removed = this.components.delete(key);
    if (removed) {
      World.informChangesFor(this);
    }
  }

  /**
   * Returns all components attached to this entity.
   */
  getAllComponents(): Component[] {
    return Array.from(this.components.values());
  }

  /**
   * Returns the number of components attached to this entity.
   */
  getCount(): number {
    return this.components.size;
  }

  recycle() {
    this.components.clear();
  }

  /**
   * Implicit conversion to string - returns the entity ID.
   * Allows NTT to be used wherever a string is expected.
   */
  toString(): string {
    return this.id;
  }

  /**
   * Returns primitive value for type coercion.
   * Enables implicit string conversion in expressions.
   */
  valueOf(): string {
    return this.id;
  }

  /**
   * Destroys this entity, removing it from the World.
   */
  destroy(): void {
    World.destroyEntity(this);
  }
}
