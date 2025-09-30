import { ComponentStatePacket } from "../network/packets/ComponentStatePacket";
import { World } from "../ecs/core/World";
import { EntityType } from "../ecs/core/types";
import { ShipPartComponent } from "../ecs/components/ShipPartComponent";
import { ParentChildComponent } from "../ecs/components/ParentChildComponent";
import { ColorComponent } from "../ecs/components/ColorComponent";
import { NetworkManager } from "../network/NetworkManager";

export interface ShipPartData {
  type: "hull" | "shield" | "engine";
  shape: "triangle" | "square";
  rotation: number;
}

export class ShipPartManager {
  private networkManager: NetworkManager;
  private localPlayerId: string | null = null;
  private gridToEntityMap: Map<string, string> = new Map(); // Grid key to entity ID

  constructor(networkManager: NetworkManager) {
    this.networkManager = networkManager;
  }

  setLocalPlayerId(playerId: string): void {
    this.localPlayerId = playerId;
  }

  private getGridKey(gridX: number, gridY: number): string {
    return `${gridX},${gridY}`;
  }

  /**
   * Creates a ship part entity locally and notifies the server
   */
  createShipPart(
    gridX: number,
    gridY: number,
    partData: ShipPartData,
  ): string | null {
    if (!this.localPlayerId) {
      console.warn("Cannot create ship part: no local player ID set");
      return null;
    }

    // Convert client part types to server values
    const type =
      partData.type === "hull" ? 0 : partData.type === "shield" ? 1 : 2;
    const shape = partData.shape === "triangle" ? 1 : 2;
    const rotation = partData.rotation || 0;

    // Create local ship part entity
    const partEntityId = crypto.randomUUID();
    const partEntity = World.createEntity(EntityType.ShipPart, partEntityId);

    // Add ShipPartComponent
    const shipPartComponent = new ShipPartComponent(partEntityId, {
      gridX,
      gridY,
      type,
      shape,
      rotation,
    });
    partEntity.set(shipPartComponent);

    // Add ParentChildComponent
    const parentChildComponent = new ParentChildComponent(
      partEntityId,
      this.localPlayerId,
    );
    partEntity.set(parentChildComponent);

    // Add ColorComponent
    const color = ColorComponent.getPartColor(partData.type);
    const colorComponent = new ColorComponent(partEntityId, color);
    partEntity.set(colorComponent);

    // Track this entity by grid position
    const gridKey = this.getGridKey(gridX, gridY);
    this.gridToEntityMap.set(gridKey, partEntityId);

    // Send ComponentStatePackets to server
    this.sendShipPartToServer(
      partEntityId,
      gridX,
      gridY,
      type,
      shape,
      rotation,
    );
    this.sendParentChildToServer(partEntityId, this.localPlayerId);
    this.sendColorToServer(partEntityId, color);

    return partEntityId;
  }

  /**
   * Removes a ship part entity locally and notifies the server
   */
  removeShipPart(gridX: number, gridY: number): boolean {
    const gridKey = this.getGridKey(gridX, gridY);
    const entityId = this.gridToEntityMap.get(gridKey);

    if (!entityId) {
      console.warn(`No ship part found at grid position (${gridX}, ${gridY})`);
      return false;
    }

    // Remove from our tracking map
    this.gridToEntityMap.delete(gridKey);

    // Remove the entity locally
    const entity = World.getEntity(entityId);
    if (entity) {
      World.destroyEntity(entity);
    }

    // Send removal packet to server (we'll send a DeathTag component)
    this.sendShipPartRemovalToServer(entityId);

    return true;
  }

  private sendShipPartRemovalToServer(entityId: string): void {
    // Send a ComponentStatePacket with DeathTag to request removal
    const packet = ComponentStatePacket.createDeathTag(
      entityId,
      this.localPlayerId || "",
    );
    this.networkManager.send(packet);
  }

  private sendShipPartToServer(
    entityId: string,
    gridX: number,
    gridY: number,
    type: number,
    shape: number,
    rotation: number,
  ): void {
    const packet = ComponentStatePacket.createShipPart(
      entityId,
      gridX,
      gridY,
      type,
      shape,
      rotation,
    );
    this.networkManager.send(packet);
  }

  private sendParentChildToServer(entityId: string, parentId: string): void {
    const packet = ComponentStatePacket.createParentChild(entityId, parentId);
    this.networkManager.send(packet);
  }

  private sendColorToServer(entityId: string, color: number): void {
    const packet = ComponentStatePacket.createColor(entityId, color);
    this.networkManager.send(packet);
  }
}
