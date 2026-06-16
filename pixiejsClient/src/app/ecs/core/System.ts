import { NTT } from "./NTT";
import { Component } from "./Component";
/**
 * Base class for all ECS systems - aligned with server NttECS architecture.
 * Systems maintain their own filtered entity lists for O(1) iteration performance.
 *
 * Key architecture:
 * - Systems maintain filtered entity lists (no repeated queries)
 * - Automatic entity tracking via entityChanged()
 * - Change processing happens between systems (like server)
 */
export abstract class System {
  /** Entities matching this system's filter */
  protected _entities = new Set<NTT>();
  /** List view for efficient iteration */
  protected _entitiesList: NTT[] = [];

  /**
   * Called when an entity's components change.
   * Automatically adds/removes entity from this system's filtered list.
   */
  entityChanged(entity: NTT): void {
    const matches = this.matchesFilter(entity);

    if (matches) {
      if (!this._entities.has(entity)) {
        this._entities.add(entity);
        this._entitiesList.push(entity);
      }
    } else {
      if (this._entities.has(entity)) {
        this._entities.delete(entity);
        // Swap-remove like server's SwapList
        const idx = this._entitiesList.indexOf(entity);
        if (idx !== -1) {
          this._entitiesList[idx] = this._entitiesList[this._entitiesList.length - 1];
          this._entitiesList.pop();
        }
      }
    }
  }

  /**
   * Override to define which entities this system processes.
   * Default: all entities
   */
  protected matchesFilter(_entity: NTT): boolean {
    return true;
  }

  /**
   * Main update entry point. Called by World.
   * Override in derived classes to implement system logic.
   */
  abstract beginUpdate(deltaTime: number): void;

  /**
   * Optional initialization hook
   */
  initialize?(): void;

  /**
   * Optional cleanup hook
   */
  cleanup?(): void;
}

/**
 * System that processes entities with exactly 1 component type.
 * Provides type-safe component access in updateEntity().
 */
export abstract class System1<T1 extends Component> extends System {
  constructor(private readonly c1Type: new (ntt: NTT) => T1) {
    super();
  }

  protected matchesFilter(entity: NTT): boolean {
    return entity.has(this.c1Type);
  }

  beginUpdate(deltaTime: number): void {
    for (const entity of this._entitiesList) {
      const c1 = entity.get(this.c1Type)!;
      this.updateEntity(entity, c1, deltaTime);
    }
  }

  /**
   * Process a single entity with its component.
   * Components are guaranteed to exist (non-null).
   */
  protected abstract updateEntity(entity: NTT, c1: T1, deltaTime: number): void;
}

/**
 * System that processes entities with exactly 2 component types.
 * Provides type-safe component access in updateEntity().
 */
export abstract class System2<T1 extends Component, T2 extends Component> extends System {
  constructor(
    private readonly c1Type: new (ntt: NTT) => T1,
    private readonly c2Type: new (ntt: NTT) => T2,
  ) {
    super();
  }

  protected matchesFilter(entity: NTT): boolean {
    return entity.has(this.c1Type) && entity.has(this.c2Type);
  }

  beginUpdate(deltaTime: number): void {
    for (const entity of this._entitiesList) {
      const c1 = entity.get(this.c1Type)!;
      const c2 = entity.get(this.c2Type)!;
      this.updateEntity(entity, c1, c2, deltaTime);
    }
  }

  /**
   * Process a single entity with its components.
   * Components are guaranteed to exist (non-null).
   */
  protected abstract updateEntity(entity: NTT, c1: T1, c2: T2, deltaTime: number): void;
}

/**
 * System that processes entities with exactly 3 component types.
 * Provides type-safe component access in updateEntity().
 */
export abstract class System3<T1 extends Component, T2 extends Component, T3 extends Component> extends System {
  constructor(
    private readonly c1Type: new (ntt: NTT) => T1,
    private readonly c2Type: new (ntt: NTT) => T2,
    private readonly c3Type: new (ntt: NTT) => T3,
  ) {
    super();
  }

  protected matchesFilter(entity: NTT): boolean {
    return entity.has(this.c1Type) && entity.has(this.c2Type) && entity.has(this.c3Type);
  }

  beginUpdate(deltaTime: number): void {
    for (const entity of this._entitiesList) {
      const c1 = entity.get(this.c1Type)!;
      const c2 = entity.get(this.c2Type)!;
      const c3 = entity.get(this.c3Type)!;
      this.updateEntity(entity, c1, c2, c3, deltaTime);
    }
  }

  /**
   * Process a single entity with its components.
   * Components are guaranteed to exist (non-null).
   */
  protected abstract updateEntity(entity: NTT, c1: T1, c2: T2, c3: T3, deltaTime: number): void;
}

/**
 * System that processes entities with exactly 4 component types.
 * Provides type-safe component access in updateEntity().
 */
export abstract class System4<T1 extends Component, T2 extends Component, T3 extends Component, T4 extends Component> extends System {
  constructor(
    private readonly c1Type: new (ntt: NTT) => T1,
    private readonly c2Type: new (ntt: NTT) => T2,
    private readonly c3Type: new (ntt: NTT) => T3,
    private readonly c4Type: new (ntt: NTT) => T4,
  ) {
    super();
  }

  protected matchesFilter(entity: NTT): boolean {
    return entity.has(this.c1Type) && entity.has(this.c2Type) && entity.has(this.c3Type) && entity.has(this.c4Type);
  }

  beginUpdate(deltaTime: number): void {
    for (const entity of this._entitiesList) {
      const c1 = entity.get(this.c1Type)!;
      const c2 = entity.get(this.c2Type)!;
      const c3 = entity.get(this.c3Type)!;
      const c4 = entity.get(this.c4Type)!;
      this.updateEntity(entity, c1, c2, c3, c4, deltaTime);
    }
  }

  /**
   * Process a single entity with its components.
   * Components are guaranteed to exist (non-null).
   */
  protected abstract updateEntity(entity: NTT, c1: T1, c2: T2, c3: T3, c4: T4, deltaTime: number): void;
}

/**
 * System that processes entities with exactly 5 component types.
 * Provides type-safe component access in updateEntity().
 */
export abstract class System5<
  T1 extends Component,
  T2 extends Component,
  T3 extends Component,
  T4 extends Component,
  T5 extends Component,
> extends System {
  constructor(
    private readonly c1Type: new (ntt: NTT) => T1,
    private readonly c2Type: new (ntt: NTT) => T2,
    private readonly c3Type: new (ntt: NTT) => T3,
    private readonly c4Type: new (ntt: NTT) => T4,
    private readonly c5Type: new (ntt: NTT) => T5,
  ) {
    super();
  }

  protected matchesFilter(entity: NTT): boolean {
    return entity.has(this.c1Type) && entity.has(this.c2Type) && entity.has(this.c3Type) && entity.has(this.c4Type) && entity.has(this.c5Type);
  }

  beginUpdate(deltaTime: number): void {
    for (const entity of this._entitiesList) {
      const c1 = entity.get(this.c1Type)!;
      const c2 = entity.get(this.c2Type)!;
      const c3 = entity.get(this.c3Type)!;
      const c4 = entity.get(this.c4Type)!;
      const c5 = entity.get(this.c5Type)!;
      this.updateEntity(entity, c1, c2, c3, c4, c5, deltaTime);
    }
  }

  /**
   * Process a single entity with its components.
   * Components are guaranteed to exist (non-null).
   */
  protected abstract updateEntity(entity: NTT, c1: T1, c2: T2, c3: T3, c4: T4, c5: T5, deltaTime: number): void;
}

/**
 * System that processes entities with exactly 6 component types.
 * Provides type-safe component access in updateEntity().
 */
export abstract class System6<
  T1 extends Component,
  T2 extends Component,
  T3 extends Component,
  T4 extends Component,
  T5 extends Component,
  T6 extends Component,
> extends System {
  constructor(
    private readonly c1Type: new (ntt: NTT) => T1,
    private readonly c2Type: new (ntt: NTT) => T2,
    private readonly c3Type: new (ntt: NTT) => T3,
    private readonly c4Type: new (ntt: NTT) => T4,
    private readonly c5Type: new (ntt: NTT) => T5,
    private readonly c6Type: new (ntt: NTT) => T6,
  ) {
    super();
  }

  protected matchesFilter(entity: NTT): boolean {
    return (
      entity.has(this.c1Type) &&
      entity.has(this.c2Type) &&
      entity.has(this.c3Type) &&
      entity.has(this.c4Type) &&
      entity.has(this.c5Type) &&
      entity.has(this.c6Type)
    );
  }

  beginUpdate(deltaTime: number): void {
    for (const entity of this._entitiesList) {
      const c1 = entity.get(this.c1Type)!;
      const c2 = entity.get(this.c2Type)!;
      const c3 = entity.get(this.c3Type)!;
      const c4 = entity.get(this.c4Type)!;
      const c5 = entity.get(this.c5Type)!;
      const c6 = entity.get(this.c6Type)!;
      this.updateEntity(entity, c1, c2, c3, c4, c5, c6, deltaTime);
    }
  }

  /**
   * Process a single entity with its components.
   * Components are guaranteed to exist (non-null).
   */
  protected abstract updateEntity(entity: NTT, c1: T1, c2: T2, c3: T3, c4: T4, c5: T5, c6: T6, deltaTime: number): void;
}
