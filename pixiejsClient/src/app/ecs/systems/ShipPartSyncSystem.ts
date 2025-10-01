import { System } from "../core/System";
import { Entity } from "../core/Entity";
import { World } from "../core/World";
import { ParentChildComponent } from "../components/ParentChildComponent";
import { RenderComponent, ShipPart } from "../components/RenderComponent";

/**
 * System that synchronizes ship parts from child entities to parent render components
 */
export class ShipPartSyncSystem extends System {
  readonly componentTypes = [ParentChildComponent];

  private parentChildListener: (event: Event) => void;

  constructor() {
    super();
    this.parentChildListener = this.handleParentChildUpdate.bind(this);
    window.addEventListener("parent-child-update", this.parentChildListener);
  }

  cleanup(): void {
    window.removeEventListener("parent-child-update", this.parentChildListener);
  }

  private handleParentChildUpdate(event: Event): void {
    const customEvent = event as CustomEvent;
    const { parentId } = customEvent.detail;

    // Update the parent entity's ship parts
    this.updateParentShipParts(parentId);
  }

  protected updateEntity(entity: Entity, _deltaTime: number): void {
    const parentChild = entity.get(ParentChildComponent)!;

    if (!parentChild) return;

    // Update the parent entity's ship parts when any child part changes
    this.updateParentShipParts(parentChild.parentId);
  }

  private updateParentShipParts(parentId: string): void {
    const parentEntity = World.getEntity(parentId);
    if (!parentEntity) return;

    const renderComponent = parentEntity.get(RenderComponent);
    if (!renderComponent) return;

    // Collect all child ship parts
    const shipParts: ShipPart[] = [];

    // Always include the player's base box at (0,0)
    // This represents the original player body
    shipParts.push({
      gridX: 0,
      gridY: 0,
      type: 0, // Hull type
      shape: 2, // Square/box shape
      rotation: 0, // No rotation
    });

    const allEntities = World.getAllEntities();

    for (const entity of allEntities) {
      const parentChild = entity.get(ParentChildComponent);

      if (parentChild && parentChild.parentId === parentId) {
        shipParts.push({
          gridX: parentChild.gridX,
          gridY: parentChild.gridY,
          type: 0, // Type is not stored in ParentChildComponent
          shape: parentChild.shape,
          rotation: parentChild.rotation,
        });
      }
    }

    // Update the parent's render component
    renderComponent.shipParts = shipParts;
  }
}
