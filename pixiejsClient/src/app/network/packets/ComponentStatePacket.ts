import { EvPacketReader } from "../EvPacketReader";
import { EvPacketWriter } from "../EvPacketWriter";
import { PacketHeader } from "../PacketHeader";
import { PacketId } from "../PacketHandler";
import { World } from "../../ecs/core/World";
import { EntityType } from "../../ecs/core/types";
import { ServerComponentType, ComponentTypeId } from "../../enums/ComponentIds";
import { ComponentRegistry } from "../../ecs/core/Component";

// Import all components to ensure decorators run
import * as Components from "../../ecs/components";
import { ImpactParticleManager } from "../../ecs/effects/ImpactParticleManager";

// Force evaluation of all component classes to ensure decorators run
// This guarantees all components are registered before use
Object.values(Components);

export class ComponentStatePacket {
  header: PacketHeader;
  entityId: string;
  componentId: number;
  dataLength: number;
  data: ArrayBuffer;

  constructor(
    header: PacketHeader,
    entityId: string,
    componentId: number,
    dataLength: number,
    data: ArrayBuffer,
  ) {
    this.header = header;
    this.entityId = entityId;
    this.componentId = componentId;
    this.dataLength = dataLength;
    this.data = data;
  }

  static handle(buffer: ArrayBuffer) {
    const packet = ComponentStatePacket.fromBuffer(buffer);

    // Get or create entity
    let entity = World.getEntity(packet.entityId);
    if (!entity) {
      entity = World.createEntity(EntityType.Player, packet.entityId);
    }

    // Special handling for certain component types
    const reader = new EvPacketReader(packet.data);

    // Handle components that don't use the decorator pattern yet
    switch (packet.componentId) {
      case ServerComponentType.DeathTag: {
        // DeathTag special handling
        reader.i64(); // changedTick
        const killerGuid = reader.Guid();

        console.log(`[DeathTag] Received for ${packet.entityId}`);

        // Handle entity death
        const localPlayerId = (window as any).localPlayerId;
        if (packet.entityId !== localPlayerId) {
          const parentChild = entity.get(Components.ParentChildComponent);
          if (parentChild) {
            window.dispatchEvent(
              new CustomEvent("parent-child-update", {
                detail: {
                  childId: packet.entityId,
                  parentId: parentChild.parentId,
                },
              }),
            );
          }
          World.destroyEntity(entity);
          console.log(`[DeathTag] Destroyed entity ${packet.entityId}`);
        } else {
          console.log(`[DeathTag] Skipping local player ${packet.entityId}`);
        }

        window.dispatchEvent(
          new CustomEvent("entity-death", {
            detail: { entityId: packet.entityId, killerId: killerGuid },
          }),
        );
        return;
      }

      case ServerComponentType.NameTag: {
        // NameTag special handling
        reader.i64(); // changedTick
        const nameBytes = new Uint8Array(64);
        for (let i = 0; i < 64; i++) {
          nameBytes[i] = reader.i8();
        }
        const nullIndex = nameBytes.indexOf(0);
        const nameLength = nullIndex >= 0 ? nullIndex : 64;
        const nameString = new TextDecoder().decode(
          nameBytes.subarray(0, nameLength),
        );

        window.dispatchEvent(
          new CustomEvent("player-name-update", {
            detail: { entityId: packet.entityId, name: nameString },
          }),
        );
        return;
      }

      case ServerComponentType.Level:
      case ServerComponentType.Inventory:
      case ServerComponentType.HealthRegen:
      case ServerComponentType.Weapon:
      case ServerComponentType.Input:
      case ServerComponentType.ShipPart:
        // These components are not used on client or handled elsewhere
        return;
    }

    // Get component class from registry
    const ComponentClass = ComponentRegistry.get(
      packet.componentId as ComponentTypeId,
    );
    if (!ComponentClass) {
      console.warn(`No component registered for type: ${packet.componentId}`);
      return;
    }

    // Create component from buffer
    const reader2 = new EvPacketReader(packet.data);
    const component = (ComponentClass as any).fromBuffer(
      packet.entityId,
      reader2,
    );

    // Handle special side effects
    this.handleSideEffects(
      packet.componentId as ComponentTypeId,
      component,
      entity,
    );

    // Set component on entity
    entity.set(component);
    World.notifyComponentChange(entity);
  }

  private static handleSideEffects(
    componentId: ComponentTypeId,
    component: any,
    entity: any,
  ): void {
    const localPlayerId = (window as any).localPlayerId;
    const isLocalPlayer = localPlayerId && entity.id === localPlayerId;

    switch (componentId) {
      case ServerComponentType.Box2DBody: {
        const box2d = component as Components.Box2DBodyComponent;

        // Update NetworkComponent
        let network = entity.get(Components.NetworkComponent);
        if (!network) {
          network = new Components.NetworkComponent(entity.id, {
            serverId: entity.id,
            isLocallyControlled: isLocalPlayer,
            serverPosition: box2d.lastPosition,
            serverVelocity: { x: 0, y: 0 },
            serverRotation: box2d.lastRotation,
          });
          entity.set(network);
        } else {
          network.serverPosition = box2d.lastPosition;
          network.serverVelocity = { x: 0, y: 0 };
          network.serverRotation = box2d.lastRotation;
        }
        network.updateLastServerTick(Number(box2d.changedTick));

        // Setup render component if needed
        if (!entity.has(Components.RenderComponent)) {
          entity.set(
            new Components.RenderComponent(entity.id, {
              sides: box2d.sides,
              shapeType: box2d.sides === 3 ? 1 : box2d.sides === 4 ? 2 : 0,
              color: box2d.color,
              shipParts: [],
            }),
          );
        }
        break;
      }

      case ServerComponentType.Health: {
        // Spawn impact particles on damage
        const health = component as Components.HealthComponent;
        const previous = entity.get(Components.HealthComponent);
        if (previous && health.Health < previous.Health) {
          const physics = entity.get(Components.Box2DBodyComponent);
          if (physics) {
            console.log(
              `[Impact] Spawning particles at ${physics.position.x}, ${physics.position.y} - health: ${previous.Health} -> ${health.Health}`,
            );
            ImpactParticleManager.getInstance().spawnBurst(
              physics.position.x,
              physics.position.y,
              {
                count: 25,
                color: 0xcccccc,
                speed: 12,
                lifetime: 1.2,
                size: 0.3,
              },
            );
          }
        }
        break;
      }

      case ServerComponentType.ParentChild: {
        // Update parent's RenderComponent with ship parts
        const pc = component as Components.ParentChildComponent;
        const parentEnt = World.getEntity(pc.parentId);
        if (parentEnt) {
          const renderComp = parentEnt.get(Components.RenderComponent);
          if (renderComp) {
            // Rebuild ship parts array
            const shipParts = [
              { gridX: 0, gridY: 0, type: 0, shape: 2, rotation: 0 },
            ];

            const allEntities = World.getAllEntities();
            for (const e of allEntities) {
              const childPc = e.get(Components.ParentChildComponent);
              if (childPc && childPc.parentId === pc.parentId) {
                shipParts.push({
                  gridX: childPc.gridX || 0,
                  gridY: childPc.gridY || 0,
                  type: 0,
                  shape: childPc.shape || 0,
                  rotation: childPc.rotation || 0,
                });
              }
            }

            renderComp.shipParts = shipParts;
          }
        }

        // Notify ShipPartManager that a ship part was confirmed by server
        window.dispatchEvent(
          new CustomEvent("ship-part-confirmed", {
            detail: {
              entityId: entity.id,
              parentId: pc.parentId,
              gridX: pc.gridX,
              gridY: pc.gridY,
            },
          }),
        );
        break;
      }
    }
  }

  // Create buffer from component (for sending to server)
  static toBuffer(
    entityId: string,
    component: any,
    componentType: ComponentTypeId,
  ): ArrayBuffer {
    const writer = new EvPacketWriter(PacketId.ComponentState);
    writer.Guid(entityId);
    writer.i8(componentType);

    // Get component class and create instance if needed
    const ComponentClass = ComponentRegistry.get(componentType);
    if (!ComponentClass) {
      throw new Error(`No component registered for type: ${componentType}`);
    }

    // Create component instance if needed
    let instance = component;
    if (!(component instanceof ComponentClass)) {
      instance = new (ComponentClass as any)(entityId);
      Object.assign(instance, component);
      instance.changedTick = BigInt(World.currentTick);
    }

    // Serialize component
    const componentBuffer = instance.toBuffer();
    writer.i16(componentBuffer.byteLength);

    // Write component data byte by byte
    const bytes = new Uint8Array(componentBuffer);
    for (let i = 0; i < bytes.length; i++) {
      writer.i8(bytes[i]);
    }

    writer.FinishPacket();
    return writer.ToArray();
  }

  // Simple factory methods for common components
  static createShipPart(
    entityId: string,
    gridX: number,
    gridY: number,
    type: number,
    shape: number,
    rotation: number,
  ): ArrayBuffer {
    return ComponentStatePacket.toBuffer(
      entityId,
      {
        gridX,
        gridY,
        type,
        shape,
        rotation,
      },
      ServerComponentType.ShipPart,
    );
  }

  static createParentChild(entityId: string, parentId: string): ArrayBuffer {
    // Special handling for ParentChild - server only expects parentId, not grid data
    const writer = new EvPacketWriter(PacketId.ComponentState);
    writer.Guid(entityId);
    writer.i8(ServerComponentType.ParentChild);

    // Calculate size: 8 bytes for changedTick + 16 bytes for GUID
    const componentSize = 8 + 16;
    writer.i16(componentSize);

    // Write only what server expects for ParentChild
    writer.i64(BigInt(World.currentTick)); // changedTick
    writer.Guid(parentId); // parentId

    writer.FinishPacket();
    return writer.ToArray();
  }

  static createDeathTag(entityId: string, killerId: string): ArrayBuffer {
    return ComponentStatePacket.toBuffer(
      entityId,
      { killerId },
      ServerComponentType.DeathTag,
    );
  }

  static createColor(entityId: string, color: number): ArrayBuffer {
    return ComponentStatePacket.toBuffer(
      entityId,
      { color },
      ServerComponentType.Color,
    );
  }

  static createEngine(entityId: string, maxThrust: number): ArrayBuffer {
    return ComponentStatePacket.toBuffer(
      entityId,
      {
        powerUse: maxThrust * 0.01,
        throttle: 1.0,
        maxPropulsion: maxThrust,
        rcs: true,
      },
      ServerComponentType.Engine,
    );
  }

  static createShield(
    entityId: string,
    charge: number,
    radius: number,
  ): ArrayBuffer {
    return ComponentStatePacket.toBuffer(
      entityId,
      {
        charge,
        maxCharge: charge,
        powerUse: 5.0,
        powerUseRecharge: 12.5,
        radius,
        minRadius: radius * 0.5,
        targetRadius: radius,
        rechargeRate: 10.0,
      },
      ServerComponentType.Shield,
    );
  }

  static createWeapon(
    entityId: string,
    damage: number,
    rateOfFire: number,
  ): ArrayBuffer {
    return ComponentStatePacket.toBuffer(
      entityId,
      {
        bulletDamage: damage,
        bulletCount: 1,
        bulletSize: 5,
        bulletSpeed: 50,
        powerUse: 5.0,
        frequency: 1000 / rateOfFire, // Convert RPS to milliseconds
      },
      ServerComponentType.Weapon,
    );
  }

  static createInput(
    entityId: string,
    buttonStates: number,
    mouseX: number,
    mouseY: number,
  ): ArrayBuffer {
    return ComponentStatePacket.toBuffer(
      entityId,
      {
        mouseDir: { x: mouseX, y: mouseY },
        buttonStates,
        didBoostLastFrame: false,
      },
      ServerComponentType.Input,
    );
  }

  static fromBuffer(buffer: ArrayBuffer): ComponentStatePacket {
    const reader = new EvPacketReader(buffer);
    const header = reader.Header();
    const entityId = reader.Guid();
    const componentId = reader.i8();
    const dataLength = reader.i16();

    // Read the component data
    const data = buffer.slice(
      reader.currentOffset,
      reader.currentOffset + dataLength,
    );

    return new ComponentStatePacket(
      header,
      entityId,
      componentId,
      dataLength,
      data,
    );
  }
}
