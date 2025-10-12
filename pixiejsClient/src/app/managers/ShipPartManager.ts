import { ComponentStatePacket } from "../network/packets/ComponentStatePacket";
import { World } from "../ecs/core/World";
import { ParentChildComponent } from "../ecs/components/ParentChildComponent";
import { ColorComponent } from "../ecs/components/ColorComponent";
import { RenderComponent } from "../ecs/components/RenderComponent";
import { NetworkManager } from "../network/NetworkManager";
import type { AttachedComponent } from "../ui/shipbuilder/BuildGrid";

export interface ShipPartData {
  type: "hull" | "shield" | "engine";
  shape: "triangle" | "square";
  rotation: number;
  attachedComponents?: AttachedComponent[];
}

export class ShipPartManager {
  private static instance: ShipPartManager | null = null;
  private networkManager: NetworkManager | null = null;
  private gridToEntityMap: Map<string, string> = new Map(); // Grid key to entity ID

  private constructor() {
    // Listen for ship parts being confirmed by server
    window.addEventListener("ship-part-confirmed", (event: any) => {
      const { entityId, parentId, gridX, gridY } = event.detail;
      if (parentId === World.Me?.id) {
        // Track this entity for removal later
        const gridKey = this.getGridKey(gridX, gridY);
        this.gridToEntityMap.set(gridKey, entityId);
      }
    });
  }

  static getInstance(): ShipPartManager {
    if (!ShipPartManager.instance) {
      ShipPartManager.instance = new ShipPartManager();
    }
    return ShipPartManager.instance;
  }

  /**
   * Initialize the manager with a NetworkManager reference
   */
  initialize(networkManager: NetworkManager): void {
    this.networkManager = networkManager;
  }

  private getGridKey(gridX: number, gridY: number): string {
    return `${gridX},${gridY}`;
  }

  /**
   * Updates the parent entity's RenderComponent with current ship parts
   */
  private updateParentRenderComponent(parentId: string): void {
    const parentEntity = World.getEntity(parentId);
    if (!parentEntity) return;

    const renderComponent = parentEntity.get(RenderComponent);
    if (!renderComponent) return;

    // Rebuild ship parts array from all children
    const shipParts = [];

    // Always include the base hull at (0,0)
    shipParts.push({
      gridX: 0,
      gridY: 0,
      type: 0, // Hull type
      shape: 2, // Square/box shape
      rotation: 0,
    });

    // Add all child parts
    const allEntities = World.queryEntitiesWithComponents(ParentChildComponent);
    for (const entity of allEntities) {
      const parentChild = entity.get(ParentChildComponent)!;
      if (parentChild.parentId === parentId) {
        shipParts.push({
          gridX: parentChild.gridX || 0,
          gridY: parentChild.gridY || 0,
          type: 0, // Type not stored in ParentChildComponent
          shape: parentChild.shape || 0,
          rotation: parentChild.rotation || 0,
        });
      }
    }

    renderComponent.shipParts = shipParts;
  }

  /**
   * Notify that a ship part entity was destroyed (called by DeathSystem)
   */
  notifyPartDestroyed(entityId: string): void {
    // Remove from gridToEntityMap if it exists
    for (const [key, id] of this.gridToEntityMap) {
      if (id === entityId) {
        this.gridToEntityMap.delete(key);
        console.log(`[ShipPartManager] Removed destroyed part ${entityId} from tracking`);
        break;
      }
    }
  }

  /**
   * Requests the server to create a ship part
   * The entity will only be created locally when the server responds
   */
  createShipPart(gridX: number, gridY: number, partData: ShipPartData): string | null {
    if (!this.networkManager) {
      console.warn("Cannot create ship part: NetworkManager not initialized");
      return null;
    }
    if (!World.Me) {
      console.warn("Cannot create ship part: no local player entity");
      return null;
    }

    // Convert client part types to server values
    const type = partData.type === "hull" ? 0 : partData.type === "shield" ? 1 : 2;
    const shape = partData.shape === "triangle" ? 1 : 2;
    const rotation = partData.rotation || 0;

    // Generate entity ID that will be used when server responds
    const partEntityId = crypto.randomUUID();

    // Get color for this part type
    const color = ColorComponent.getPartColor(partData.type);

    // Note: We do NOT create the entity locally here
    // We only send the request to the server
    // The entity will be created when we receive the components back from the server

    // Send ComponentStatePackets to server
    // Send ParentChild first - this establishes ownership
    this.sendParentChildToServer(partEntityId, World.Me.id);
    // Send ShipPart with the grid data
    this.sendShipPartToServer(partEntityId, gridX, gridY, type, shape, rotation);
    this.sendColorToServer(partEntityId, color);

    // Send attached component packets
    if (partData.attachedComponents) {
      for (const component of partData.attachedComponents) {
        this.sendAttachedComponentToServer(partEntityId, component);
      }
    }

    // Update parent's render component with new ship parts
    this.updateParentRenderComponent(World.Me.id);

    return partEntityId;
  }

  /**
   * Requests the server to remove a ship part
   * The entity will only be removed locally when the server confirms
   */
  removeShipPart(gridX: number, gridY: number): boolean {
    if (!World.Me) return false;

    const gridKey = this.getGridKey(gridX, gridY);
    const entityId = this.gridToEntityMap.get(gridKey);

    if (!entityId) {
      // If we don't have it in our map, try to find it by checking entities
      // This can happen if the entity was created before we started tracking
      const allEntities = World.getAllEntities();
      for (const entity of allEntities) {
        const parentChild = entity.get(ParentChildComponent);
        if (parentChild && parentChild.parentId === World.Me.id && parentChild.gridX === gridX && parentChild.gridY === gridY) {
          // Found it - send removal request
          this.sendShipPartRemovalToServer(entity.id);
          return true;
        }
      }
      console.warn(`No ship part found at grid position (${gridX}, ${gridY})`);
      return false;
    }

    // Send removal packet to server (we'll send a DeathTag component)
    // The entity will be removed when the server confirms
    this.sendShipPartRemovalToServer(entityId);

    return true;
  }

  private sendShipPartRemovalToServer(entityId: string): void {
    if (!World.Me || !this.networkManager) return;

    // Send a ComponentStatePacket with DeathTag to request removal
    const packet = ComponentStatePacket.createDeathTag(entityId, World.Me.id);
    this.networkManager.send(packet);
  }

  private sendShipPartToServer(entityId: string, gridX: number, gridY: number, type: number, shape: number, rotation: number): void {
    if (!this.networkManager) return;
    const packet = ComponentStatePacket.createShipPart(entityId, gridX, gridY, type, shape, rotation);
    this.networkManager.send(packet);
  }

  private sendParentChildToServer(entityId: string, parentId: string): void {
    if (!this.networkManager) return;
    const packet = ComponentStatePacket.createParentChild(entityId, parentId);
    this.networkManager.send(packet);
  }

  private sendColorToServer(entityId: string, color: number): void {
    if (!this.networkManager) return;
    const packet = ComponentStatePacket.createColor(entityId, color);
    this.networkManager.send(packet);
  }

  private sendAttachedComponentToServer(entityId: string, component: AttachedComponent): void {
    if (!World.Me || !this.networkManager) return;

    if (component.type === "engine") {
      const packet = ComponentStatePacket.createEngine(entityId, component.engineThrust);
      this.networkManager.send(packet);
    } else if (component.type === "shield") {
      const packet = ComponentStatePacket.createShield(entityId, component.shieldCharge, component.shieldRadius);
      this.networkManager.send(packet);
    } else if (component.type === "weapon") {
      const packet = ComponentStatePacket.createWeapon(entityId, World.Me.id, component.weaponDamage, component.weaponRateOfFire);
      this.networkManager.send(packet);
    }
  }
}
