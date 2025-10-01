import { EvPacketReader } from "../EvPacketReader";
import { EvPacketWriter } from "../EvPacketWriter";
import { PacketHeader } from "../PacketHeader";
import { PacketId } from "../PacketHandler";
import { World } from "../../ecs/core/World";
import { EntityType } from "../../ecs/core/types";
import { ComponentType, ComponentTypeId } from "../../enums/ComponentIds";
import { GravityComponent } from "../../ecs/components/GravityComponent";
import { HealthComponent } from "../../ecs/components/HealthComponent";
import { EnergyComponent } from "../../ecs/components/EnergyComponent";
import { ShieldComponent } from "../../ecs/components/ShieldComponent";
import { EngineComponent } from "../../ecs/components/EngineComponent";
import { ParentChildComponent } from "../../ecs/components/ParentChildComponent";
import { Box2DBodyComponent } from "../../ecs/components/Box2DBodyComponent";
import { NetworkComponent } from "../../ecs/components/NetworkComponent";
import { RenderComponent } from "../../ecs/components/RenderComponent";
import { ColorComponent } from "../../ecs/components/ColorComponent";
import { LifeTimeComponent } from "../../ecs/components/LifeTimeComponent";

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
        data: { gridX, gridY, type, shape, rotation },
      },
      ComponentType.ShipPart,
    );
  }

  static createParentChild(entityId: string, parentId: string): ArrayBuffer {
    return ComponentStatePacket.toBuffer(
      entityId,
      {
        parentId,
      },
      ComponentType.ParentChild,
    );
  }

  static createDeathTag(entityId: string, killerId: string): ArrayBuffer {
    return ComponentStatePacket.toBuffer(
      entityId,
      {
        killerId,
      },
      ComponentType.DeathTag,
    );
  }

  static createColor(entityId: string, color: number): ArrayBuffer {
    return ComponentStatePacket.toBuffer(
      entityId,
      {
        color,
      },
      ComponentType.Color,
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
      ComponentType.Engine,
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
      ComponentType.Shield,
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
      ComponentType.Weapon,
    );
  }

  /**
   * Serialize a component to binary format
   * Similar to server's ComponentSerializer.Serialize()
   */
  static toBuffer(
    entityId: string,
    component: any,
    componentType: ComponentTypeId,
  ): ArrayBuffer {
    const writer = new EvPacketWriter(PacketId.ComponentState);
    writer.Guid(entityId);
    writer.i8(componentType);

    switch (componentType) {
      case ComponentType.Input: {
        // InputComponent: long ChangedTick (8), Vector2 MouseDir (8), PlayerInput ButtonStates (2), bool DidBoostLastFrame (1)
        const componentDataSize = 8 + 8 + 2 + 1; // 19 bytes
        writer.i16(componentDataSize);
        writer.i64(World.currentTick);
        writer.f32(component.mouseDir.x);
        writer.f32(component.mouseDir.y);
        writer.i16(component.buttonStates);
        writer.i8(component.didBoostLastFrame ? 1 : 0);
        break;
      }

      case ComponentType.ShipPart: {
        // ShipPartComponent: long ChangedTick (8), sbyte GridX (1), sbyte GridY (1), byte Type (1), byte Shape (1), byte Rotation (1)
        const componentDataSize = 8 + 5;
        writer.i16(componentDataSize);
        writer.i64(World.currentTick);
        writer.i8(component.data.gridX);
        writer.i8(component.data.gridY);
        writer.i8(component.data.type);
        writer.i8(component.data.shape);
        writer.i8(component.data.rotation);
        break;
      }

      case ComponentType.ParentChild: {
        // ParentChildComponent: long ChangedTick (8), Guid ParentId (16), sbyte GridX (1), sbyte GridY (1), byte Shape (1), byte Rotation (1)
        const componentDataSize = 8 + 16 + 1 + 1 + 1 + 1;
        writer.i16(componentDataSize);
        writer.i64(World.currentTick);
        writer.Guid(component.parentId);
        writer.i8(component.gridX || 0);
        writer.i8(component.gridY || 0);
        writer.i8(component.shape || 0);
        writer.i8(component.rotation || 0);
        break;
      }

      case ComponentType.DeathTag: {
        // DeathTagComponent: long ChangedTick (8), Guid Killer (16)
        const componentDataSize = 8 + 16;
        writer.i16(componentDataSize);
        writer.i64(World.currentTick);
        writer.Guid(component.killerId);
        break;
      }

      case ComponentType.Color: {
        // ColorComponent: long ChangedTick (8), uint Color (4)
        const componentDataSize = 8 + 4;
        writer.i16(componentDataSize);
        writer.i64(World.currentTick);
        writer.u32(component.color);
        break;
      }

      case ComponentType.Engine: {
        // EngineComponent: long ChangedTick (8), float PowerUse (4), float Throttle (4), float MaxThrustNewtons (4), bool RCS (1)
        const componentDataSize = 8 + 4 + 4 + 4 + 1;
        writer.i16(componentDataSize);
        writer.i64(World.currentTick);
        writer.f32(component.powerUse);
        writer.f32(component.throttle);
        writer.f32(component.maxPropulsion);
        writer.i8(component.rcs ? 1 : 0);
        break;
      }

      case ComponentType.Shield: {
        // ShieldComponent: long ChangedTick (8), bool PowerOn (1), bool LastPowerOn (1),
        // float Charge (4), float MaxCharge (4), float PowerUse (4), float PowerUseRecharge (4),
        // float Radius (4), float MinRadius (4), float TargetRadius (4), float RechargeRate (4),
        // long RechargeDelayTicks (8), long LastDamageTimeTicks (8)
        const componentDataSize =
          8 + 1 + 1 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 8 + 8;
        writer.i16(componentDataSize);
        writer.i64(World.currentTick);
        writer.i8(1); // powerOn = true
        writer.i8(1); // lastPowerOn = true
        writer.f32(component.charge);
        writer.f32(component.maxCharge);
        writer.f32(component.powerUse);
        writer.f32(component.powerUseRecharge);
        writer.f32(component.radius);
        writer.f32(component.minRadius);
        writer.f32(component.targetRadius);
        writer.f32(component.rechargeRate);
        writer.i64(BigInt(0)); // rechargeDelayTicks
        writer.i64(BigInt(0)); // lastDamageTimeTicks
        break;
      }

      case ComponentType.Weapon: {
        // WeaponComponent: long ChangedTick (8), Guid Owner (16), bool Fire (1),
        // long FrequencyTicks (8), long LastShotTicks (8),
        // ushort BulletDamage (2), byte BulletCount (1), byte BulletSize (1),
        // ushort BulletSpeed (2), float PowerUse (4), Vector2 Direction (8)
        const componentDataSize = 8 + 16 + 1 + 8 + 8 + 2 + 1 + 1 + 2 + 4 + 8;
        writer.i16(componentDataSize);
        writer.i64(World.currentTick);
        writer.Guid(
          (window as any).localPlayerId ||
            "00000000-0000-0000-0000-000000000000",
        ); // owner
        writer.i8(0); // fire = false
        writer.i64(BigInt(component.frequency || 200)); // frequency in ticks
        writer.i64(BigInt(0)); // lastShotTicks
        writer.i16(component.bulletDamage);
        writer.i8(component.bulletCount);
        writer.i8(component.bulletSize);
        writer.i16(component.bulletSpeed);
        writer.f32(component.powerUse);
        writer.f32(1); // direction.x
        writer.f32(0); // direction.y
        break;
      }

      default:
        throw new Error(
          `Unsupported component type for serialization: ${componentType}`,
        );
    }

    writer.FinishPacket();
    return writer.ToArray();
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
      ComponentType.Input,
    );
  }

  static handle(buffer: ArrayBuffer) {
    const packet = ComponentStatePacket.fromBuffer(buffer);

    // Get or create entity
    let entity = World.getEntity(packet.entityId);
    if (!entity) {
      entity = World.createEntity(EntityType.Player, packet.entityId);
    }

    // Check if this is the local player
    const localPlayerId = (window as any).localPlayerId;
    const isLocalPlayer = localPlayerId && packet.entityId === localPlayerId;

    // Deserialize based on component ID
    const reader = new EvPacketReader(packet.data);

    // All component packets start with ChangedTick - read it once
    const serverTick = Number(reader.i64());

    // Update network component with server tick
    let network = entity.get(NetworkComponent);
    if (network) {
      network.updateLastServerTick(serverTick);
    }

    switch (packet.componentId) {
      case ComponentType.Box2DBody:
        // Read Box2DBody component matching server struct layout (Pack=1, no padding):
        // B2BodyId (8), bool IsStatic (1), uint Color (4),
        // float Density (4), int Sides (4), Vector2 LastPosition (8), float LastRotation (4)
        // Skip B2BodyId (8 bytes - two int32s)
        reader.Skip(8);
        reader.i8(); // isStatic
        // NO padding - Pack=1
        const color = reader.u32();
        const density = reader.f32();
        const sides = reader.i32();

        // Read LastPosition and LastRotation
        const lastPositionX = reader.f32();
        const lastPositionY = reader.f32();
        const lastRotation = reader.f32();

        // Use LastPosition as the actual position since there's no cached position in the struct
        const positionX = lastPositionX;
        const positionY = lastPositionY;
        const rotation = lastRotation;
        // No velocity data in the struct, default to zero
        const velocityX = 0;
        const velocityY = 0;

        // Set up or update physics component with actual position/velocity data
        if (!entity.has(Box2DBodyComponent)) {
          // All entities use 1x1 unit size to match server
          const physics = new Box2DBodyComponent(entity.id, {
            position: { x: positionX, y: positionY },
            velocity: { x: velocityX, y: velocityY },
            acceleration: { x: 0, y: 0 },
            size: 1.0,
            width: 1.0,
            height: 1.0,
            drag: 0.002,
            density: density,
            elasticity: 0.8,
          });
          physics.setRotation(rotation);
          entity.set(physics);
        } else {
          // Update existing physics component with new position/velocity
          const physics = entity.get(Box2DBodyComponent);
          if (physics) {
            physics.position = { x: positionX, y: positionY };
            physics.linearVelocity = { x: velocityX, y: velocityY };
            physics.setRotation(rotation);
          }
        }

        // Set up or update network component with position/velocity data
        if (!entity.has(NetworkComponent)) {
          network = new NetworkComponent(entity.id, {
            serverId: packet.entityId,
            isLocallyControlled: isLocalPlayer,
            serverPosition: { x: positionX, y: positionY },
            serverVelocity: { x: velocityX, y: velocityY },
            serverRotation: rotation,
          });
          network.updateLastServerTick(serverTick);
          entity.set(network);
        } else {
          // Update existing network component with new server state
          if (network) {
            network.serverPosition = { x: positionX, y: positionY };
            network.serverVelocity = { x: velocityX, y: velocityY };
            network.serverRotation = rotation;
          }
        }

        // Notify the world that this entity has been updated
        World.notifyComponentChange(entity);

        // Set up render component
        if (!entity.has(RenderComponent)) {
          // Derive shapeType from sides
          const derivedShapeType = sides === 3 ? 1 : sides === 4 ? 2 : 0; // Triangle=1, Box=2, Circle=0
          const render = new RenderComponent(entity.id, {
            sides: sides,
            shapeType: derivedShapeType,
            color: color,
            shipParts: [], // Will be populated by ship part entities
          });
          entity.set(render);
        }
        break;

      case ComponentType.Gravity:
        const strength = reader.f32();
        const radius = reader.f32();

        entity.set(new GravityComponent(packet.entityId, strength, radius));
        break;

      case ComponentType.Health:
        const health = reader.f32();
        const maxHealth = reader.f32();

        entity.set(new HealthComponent(packet.entityId, health, maxHealth));
        break;

      case ComponentType.Energy:
        reader.f32(); // _dischargeRateAcc
        const dischargeRate = reader.f32();
        const chargeRate = reader.f32();
        const availableCharge = reader.f32();
        const batteryCapacity = reader.f32();

        entity.set(
          new EnergyComponent(packet.entityId, {
            batteryCapacity,
            availableCharge,
            chargeRate,
            dischargeRate,
          }),
        );
        break;

      case ComponentType.Shield:
        // Server struct (Pack=1): PowerOn (1), LastPowerOn (1),
        // Charge (4), MaxCharge (4), PowerUse (4), PowerUseRecharge (4),
        // Radius (4), MinRadius (4), TargetRadius (4), RechargeRate (4),
        // RechargeDelayTicks (8), LastDamageTimeTicks (8)
        const powerOn = reader.i8() !== 0;
        const lastPowerOn = reader.i8() !== 0;
        // NO padding with Pack=1
        const charge = reader.f32();
        const maxCharge = reader.f32();
        const powerUse = reader.f32();
        const powerUseRecharge = reader.f32();
        const shieldRadius = reader.f32();
        const minRadius = reader.f32();
        const targetRadius = reader.f32();
        const rechargeRate = reader.f32();
        reader.i64(); // _rechargeDelayTicks
        reader.i64(); // _lastDamageTimeTicks

        // Update existing component or create new one
        let shieldComponent = entity.get(ShieldComponent);
        if (!shieldComponent) {
          shieldComponent = new ShieldComponent(packet.entityId, {
            charge,
            maxCharge,
            powerUse,
            powerUseRecharge,
            radius: shieldRadius,
            minRadius,
            targetRadius,
            rechargeRate,
          });
          entity.set(shieldComponent);
        } else {
          // Update all fields on existing component
          shieldComponent.charge = charge;
          shieldComponent.maxCharge = maxCharge;
          shieldComponent.powerUse = powerUse;
          shieldComponent.powerUseRecharge = powerUseRecharge;
          shieldComponent.radius = shieldRadius;
          shieldComponent.minRadius = minRadius;
          shieldComponent.targetRadius = targetRadius;
          shieldComponent.rechargeRate = rechargeRate;
        }
        shieldComponent.powerOn = powerOn;
        shieldComponent.lastPowerOn = lastPowerOn;
        break;

      case ComponentType.Engine:
        // Server struct (Pack=1): PowerUse (4), Throttle (4),
        // MaxThrustNewtons (4), RCS (1)
        const enginePowerUse = reader.f32();
        const throttle = reader.f32();
        const maxThrustNewtons = reader.f32();
        const rcs = reader.i8() !== 0;

        entity.set(
          new EngineComponent(packet.entityId, {
            maxPropulsion: maxThrustNewtons,
            powerUse: enginePowerUse,
            throttle,
            rcs,
          }),
        );
        break;

      case ComponentType.Level:
        reader.i32(); // level
        reader.i32(); // expToNextLevel
        reader.i32(); // experience

        break;

      case ComponentType.Inventory:
        reader.i32(); // totalCapacity
        reader.i32(); // triangles
        reader.i32(); // squares
        reader.i32(); // pentagons

        break;

      case ComponentType.NameTag:
        const nameBytes = new Uint8Array(64);
        for (let i = 0; i < 64; i++) {
          nameBytes[i] = reader.i8();
        }
        const nullIndex = nameBytes.indexOf(0);
        const nameLength = nullIndex >= 0 ? nullIndex : 64;
        const nameString = new TextDecoder().decode(
          nameBytes.subarray(0, nameLength),
        );

        const nameEvent = new CustomEvent("player-name-update", {
          detail: { entityId: packet.entityId, name: nameString },
        });
        window.dispatchEvent(nameEvent);
        break;

      case ComponentType.DeathTag:
        const killerGuid = reader.Guid();

        console.log(`[DeathTag] Received for ${packet.entityId}`);

        // Handle entity death - remove from world
        const deadEntity = World.getEntity(packet.entityId);
        if (deadEntity) {
          const localPlayerId = (window as any).localPlayerId;
          if (packet.entityId !== localPlayerId) {
            // Check if this is a ship part with a parent - trigger cleanup
            const parentChild = deadEntity.get(ParentChildComponent);
            if (parentChild) {
              // Trigger parent-child update to clean up ship parts from parent render
              const cleanupEvent = new CustomEvent("parent-child-update", {
                detail: {
                  childId: packet.entityId,
                  parentId: parentChild.parentId,
                },
              });
              window.dispatchEvent(cleanupEvent);
            }

            World.destroyEntity(deadEntity);
            console.log(`[DeathTag] Destroyed entity ${packet.entityId}`);
          } else {
            console.log(`[DeathTag] Skipping local player ${packet.entityId}`);
          }
        } else {
          console.log(`[DeathTag] Entity ${packet.entityId} not found`);
        }

        const deathEvent = new CustomEvent("entity-death", {
          detail: { entityId: packet.entityId, killerId: killerGuid },
        });
        window.dispatchEvent(deathEvent);
        break;

      case ComponentType.ParentChild:
        const parentId = reader.Guid();
        const pcGridX = reader.i8();
        const pcGridY = reader.i8();
        const pcShape = reader.i8();
        const pcRotation = reader.i8();

        // Create parent-child entity with component including ship part data
        let childEntity = World.getEntity(packet.entityId);
        if (!childEntity) {
          childEntity = World.createEntity(
            EntityType.ShipPart,
            packet.entityId,
          );
        }

        const parentChildComponent = new ParentChildComponent(packet.entityId, {
          parentId,
          gridX: pcGridX,
          gridY: pcGridY,
          shape: pcShape,
          rotation: pcRotation,
        });
        childEntity.set(parentChildComponent);

        // Update parent's RenderComponent directly
        const parentEnt = World.getEntity(parentId);
        if (parentEnt) {
          const renderComp = parentEnt.get(RenderComponent);
          if (renderComp) {
            // Rebuild ship parts array
            const shipParts = [
              {
                gridX: 0,
                gridY: 0,
                type: 0,
                shape: 2,
                rotation: 0,
              },
            ];

            const allEntities = World.getAllEntities();
            for (const e of allEntities) {
              const pc = e.get(ParentChildComponent);
              if (pc && pc.parentId === parentId) {
                shipParts.push({
                  gridX: pc.gridX || 0,
                  gridY: pc.gridY || 0,
                  type: 0,
                  shape: pc.shape || 0,
                  rotation: pc.rotation || 0,
                });
              }
            }

            renderComp.shipParts = shipParts;
          }
        }
        break;

      case ComponentType.Color:
        const colorValue = reader.u32();

        entity.set(new ColorComponent(packet.entityId, colorValue));
        break;

      case ComponentType.Lifetime:
        const lifetimeSeconds = reader.f32();

        entity.set(new LifeTimeComponent(packet.entityId, lifetimeSeconds));
        break;

      case ComponentType.HealthRegen:
        // HealthRegenComponent: float PassiveHealPerSec (4)
        reader.f32(); // _passiveHealPerSec
        // Client doesn't need to track health regen - server handles it
        break;

      case ComponentType.Weapon:
        // WeaponComponent: NTT Owner (16), bool Fire (1),
        // TimeSpan Frequency (8), TimeSpan LastShot (8), ushort BulletDamage (2),
        // byte BulletCount (1), byte BulletSize (1), ushort BulletSpeed (2),
        // float PowerUse (4), Vector2 Direction (8)
        reader.Skip(16); // NTT Owner
        reader.i8(); // _fire
        reader.Skip(8); // TimeSpan Frequency
        reader.Skip(8); // TimeSpan LastShot
        reader.u16(); // _bulletDamage
        reader.i8(); // _bulletCount
        reader.i8(); // _bulletSize
        reader.u16(); // _bulletSpeed
        reader.f32(); // _powerUse
        reader.Skip(8); // Vector2 Direction
        // Client doesn't need weapon component data - server handles shooting
        break;

      default:
        console.warn(`Unknown component ID: ${packet.componentId}`);
        break;
    }
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
