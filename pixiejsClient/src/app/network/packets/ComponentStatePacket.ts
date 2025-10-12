import { EvPacketReader } from "../EvPacketReader";
import { EvPacketWriter } from "../EvPacketWriter";
import { PacketHeader } from "../PacketHeader";
import { PacketId } from "../PacketHandler";
import { World } from "../../ecs/core/World";
import { EntityType } from "../../ecs/core/types";
import { ServerComponentType, ComponentTypeId } from "../../enums/ComponentIds";
import { ComponentRegistry } from "../../ecs/core/Component";
import { PlayerNameManager } from "../../managers/PlayerNameManager";

import * as Components from "../../ecs/components";

Object.values(Components);

export class ComponentStatePacket {
  header: PacketHeader;
  entityId: string;
  componentId: number;
  dataLength: number;
  data: ArrayBuffer;

  constructor(header: PacketHeader, entityId: string, componentId: number, dataLength: number, data: ArrayBuffer) {
    this.header = header;
    this.entityId = entityId;
    this.componentId = componentId;
    this.dataLength = dataLength;
    this.data = data;
  }

  static handle(buffer: ArrayBuffer) {
    const packet = ComponentStatePacket.fromBuffer(buffer);

    let entity = World.getEntity(packet.entityId);
    if (!entity) {
      entity = World.createEntity(EntityType.Player, packet.entityId);
    }

    const reader = new EvPacketReader(packet.data);

    switch (packet.componentId) {
      case ServerComponentType.NameTag: {
        reader.i64();
        const nameBytes = new Uint8Array(64);
        for (let i = 0; i < 64; i++) {
          nameBytes[i] = reader.i8();
        }
        const nullIndex = nameBytes.indexOf(0);
        const nameLength = nullIndex >= 0 ? nullIndex : 64;
        const nameString = new TextDecoder().decode(nameBytes.subarray(0, nameLength));

        PlayerNameManager.getInstance().setPlayerName(packet.entityId, nameString);
        return;
      }
    }

    const ComponentClass = ComponentRegistry.get(packet.componentId as ComponentTypeId);
    if (!ComponentClass) {
      console.warn(`No component registered for type: ${packet.componentId}`);
      return;
    }

    const reader2 = new EvPacketReader(packet.data);
    const component = (ComponentClass as any).fromBuffer(packet.entityId, reader2);

    this.handleSideEffects(packet.componentId as ComponentTypeId, component, entity);

    entity.set(component);
  }

  private static handleSideEffects(componentId: ComponentTypeId, component: any, entity: any): void {
    const localPlayerId = (window as any).localPlayerId;
    const isLocalPlayer = localPlayerId && entity.id === localPlayerId;

    switch (componentId) {
      case ServerComponentType.Physics: {
        const box2d = component as Components.PhysicsComponent;

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

      case ServerComponentType.Color: {
        const colorComp = component as Components.ColorComponent;
        let renderComp = entity.get(Components.RenderComponent);

        if (!renderComp) {
          renderComp = new Components.RenderComponent(entity.id, {
            sides: 4,
            shapeType: 2,
            color: colorComp.color,
            shipParts: [],
          });
          entity.set(renderComp);
        } else {
          renderComp.color = colorComp.color;
        }
        break;
      }

      case ServerComponentType.ParentChild: {
        const pc = component as Components.ParentChildComponent;

        if (!entity.has(Components.RenderComponent)) {
          entity.set(
            new Components.RenderComponent(entity.id, {
              sides: 4,
              shapeType: pc.shape === 1 ? 1 : 2,
              color: 0xffffff,
              shipParts: [],
            }),
          );
        }

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

  static toBuffer(entityId: string, component: any, componentType: ComponentTypeId): ArrayBuffer {
    const writer = new EvPacketWriter(PacketId.ComponentState);
    writer.Guid(entityId);
    writer.i8(componentType);

    const ComponentClass = ComponentRegistry.get(componentType);
    if (!ComponentClass) {
      throw new Error(`No component registered for type: ${componentType}`);
    }

    let instance = component;
    if (!(component instanceof ComponentClass)) {
      instance = new (ComponentClass as any)(entityId);
      Object.assign(instance, component);
      instance.changedTick = BigInt(World.currentTick);
    }

    const componentBuffer = instance.toBuffer();
    writer.i16(componentBuffer.byteLength);

    const bytes = new Uint8Array(componentBuffer);
    for (let i = 0; i < bytes.length; i++) {
      writer.i8(bytes[i]);
    }

    writer.FinishPacket();
    return writer.ToArray();
  }

  static createShipPart(entityId: string, gridX: number, gridY: number, type: number, shape: number, rotation: number): ArrayBuffer {
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
    const writer = new EvPacketWriter(PacketId.ComponentState);
    writer.Guid(entityId);
    writer.i8(ServerComponentType.ParentChild);

    const componentSize = 8 + 16;
    writer.i16(componentSize);

    writer.i64(BigInt(World.currentTick));
    writer.Guid(parentId);

    writer.FinishPacket();
    return writer.ToArray();
  }

  static createDeathTag(entityId: string, killerId: string): ArrayBuffer {
    return ComponentStatePacket.toBuffer(entityId, { killerId }, ServerComponentType.DeathTag);
  }

  static createColor(entityId: string, color: number): ArrayBuffer {
    return ComponentStatePacket.toBuffer(entityId, { color }, ServerComponentType.Color);
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

  static createShield(entityId: string, charge: number, radius: number): ArrayBuffer {
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

  static createWeapon(entityId: string, ownerId: string, damage: number, rateOfFire: number): ArrayBuffer {
    return ComponentStatePacket.toBuffer(
      entityId,
      {
        owner: ownerId,
        fire: false,
        bulletDamage: damage,
        bulletCount: 1,
        bulletSize: 5,
        bulletSpeed: 50,
        powerUse: 5.0,
        frequency: 1000 / rateOfFire,
        lastShot: 0n,
        direction: { x: 1, y: 0 },
      },
      ServerComponentType.Weapon,
    );
  }

  static createInput(buttonStates: number, mouseX: number, mouseY: number): ArrayBuffer {
    return ComponentStatePacket.toBuffer(
      World.Me!.id,
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

    const data = buffer.slice(reader.currentOffset, reader.currentOffset + dataLength);

    return new ComponentStatePacket(header, entityId, componentId, dataLength, data);
  }
}
