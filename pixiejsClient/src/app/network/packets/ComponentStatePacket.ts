import { EvPacketReader } from "../EvPacketReader";
import { EvPacketWriter } from "../EvPacketWriter";
import { PacketHeader } from "../PacketHeader";
import { PacketId } from "../PacketHandler";
import { World } from "../../ecs/core/World";
import { EntityType } from "../../ecs/core/types";
import { ComponentType } from "../../enums/ComponentIds";
import { GravityComponent } from "../../ecs/components/GravityComponent";
import { HealthComponent } from "../../ecs/components/HealthComponent";
import { EnergyComponent } from "../../ecs/components/EnergyComponent";
import { ShieldComponent } from "../../ecs/components/ShieldComponent";
import { EngineComponent } from "../../ecs/components/EngineComponent";
import { ShipPartComponent } from "../../ecs/components/ShipPartComponent";
import { ParentChildComponent } from "../../ecs/components/ParentChildComponent";
import { PhysicsComponent } from "../../ecs/components/PhysicsComponent";
import { NetworkComponent } from "../../ecs/components/NetworkComponent";
import { RenderComponent } from "../../ecs/components/RenderComponent";
import { ColorComponent } from "../../ecs/components/ColorComponent";
import { LifetimeComponent } from "../../ecs/components/LifetimeComponent";

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
    const writer = new EvPacketWriter(PacketId.ComponentState);
    writer.Guid(entityId);
    writer.i8(ComponentType.ShipPart);

    // Calculate component data size (changedTick + 5 bytes for ship part data)
    const componentDataSize = 8 + 5; // 8 bytes for i64 changedTick + 5 bytes for part data
    writer.i16(componentDataSize);

    // Write component data (matching server ShipPartComponent layout)
    writer.i64(World.currentTick); // changedTick
    writer.i8(gridX); // GridX (sbyte)
    writer.i8(gridY); // GridY (sbyte)
    writer.i8(type); // Type (byte)
    writer.i8(shape); // Shape (byte)
    writer.i8(rotation); // Rotation (byte)

    writer.FinishPacket();
    return writer.ToArray();
  }

  static createParentChild(entityId: string, parentId: string): ArrayBuffer {
    const writer = new EvPacketWriter(PacketId.ComponentState);
    writer.Guid(entityId);
    writer.i8(ComponentType.ParentChild);

    // Calculate component data size (changedTick + 16 bytes for parent GUID)
    const componentDataSize = 8 + 16; // 8 bytes for i64 changedTick + 16 bytes for parent GUID
    writer.i16(componentDataSize);

    // Write component data (matching server ParentChildComponent layout)
    writer.i64(World.currentTick); // changedTick
    writer.Guid(parentId); // ParentId (NTT/Guid)

    writer.FinishPacket();
    return writer.ToArray();
  }

  static createDeathTag(entityId: string, killerId: string): ArrayBuffer {
    const writer = new EvPacketWriter(PacketId.ComponentState);
    writer.Guid(entityId);
    writer.i8(ComponentType.DeathTag);

    // Calculate component data size (changedTick + 16 bytes for killer GUID)
    const componentDataSize = 8 + 16; // 8 bytes for i64 changedTick + 16 bytes for killer GUID
    writer.i16(componentDataSize);

    // Write component data (matching server DeathTagComponent layout)
    writer.i64(World.currentTick); // changedTick
    writer.Guid(killerId); // Killer (NTT/Guid)

    writer.FinishPacket();
    return writer.ToArray();
  }

  static createColor(entityId: string, color: number): ArrayBuffer {
    const writer = new EvPacketWriter(PacketId.ComponentState);
    writer.Guid(entityId);
    writer.i8(ComponentType.Color);

    // Calculate component data size (changedTick + 4 bytes for color)
    const componentDataSize = 8 + 4; // 8 bytes for i64 changedTick + 4 bytes for uint color
    writer.i16(componentDataSize);

    // Write component data (matching server ColorComponent layout)
    writer.i64(World.currentTick); // changedTick
    writer.u32(color); // Color (uint)

    writer.FinishPacket();
    return writer.ToArray();
  }

  static createInput(
    entityId: string,
    buttonStates: number,
    mouseX: number,
    mouseY: number,
  ): ArrayBuffer {
    const writer = new EvPacketWriter(PacketId.ComponentState);
    writer.Guid(entityId);
    writer.i8(ComponentType.Input);

    // InputComponent: long ChangedTick (8), Vector2 MovementAxis (8), Vector2 MouseDir (8), PlayerInput ButtonStates (2), bool DidBoostLastFrame (1)
    const componentDataSize = 8 + 8 + 8 + 2 + 1; // 27 bytes
    writer.i16(componentDataSize);

    // Write component data (matching server InputComponent layout)
    writer.i64(World.currentTick); // changedTick
    writer.f32(0); // MovementAxis.X (unused - server calculates from buttons)
    writer.f32(0); // MovementAxis.Y (unused - server calculates from buttons)
    writer.f32(mouseX); // MouseDir.X
    writer.f32(mouseY); // MouseDir.Y
    writer.i16(buttonStates); // ButtonStates (ushort on server, but i16 works)
    writer.i8(0); // DidBoostLastFrame (server tracks this)

    writer.FinishPacket();
    return writer.ToArray();
  }

  static handle(buffer: ArrayBuffer) {
    const packet = ComponentStatePacket.fromBuffer(buffer);

    // Get or create entity
    let entity = World.getEntity(packet.entityId);
    if (!entity) {
      entity = World.createEntity(EntityType.Player, packet.entityId);
    }

    // Check if this is the local player AFTER ensuring entity exists
    const localPlayerId = (window as any).localPlayerId;
    const isLocalPlayer = localPlayerId && packet.entityId === localPlayerId;

    // Log components received for local player (disabled to prevent console spam)
    // if (isLocalPlayer) {
    //   const componentNames: { [key: number]: string } = {
    //     1: "Box2DBody",
    //     2: "Gravity",
    //     3: "Health",
    //     4: "Energy",
    //     5: "Shield",
    //     6: "Engine",
    //     7: "Level",
    //     8: "Inventory",
    //     9: "NameTag",
    //     10: "DeathTag",
    //     11: "ShipPart",
    //     12: "ParentChild",
    //     13: "Color",
    //     14: "Lifetime",
    //   };
    //   console.log(
    //     `[LOCAL PLAYER] Component ${componentNames[packet.componentId] || packet.componentId} received`,
    //   );
    // }

    // Deserialize based on component ID
    const reader = new EvPacketReader(packet.data);

    // Only log Box2DBody components for the local player
    const isPlayerBox2DBody =
      isLocalPlayer && packet.componentId === ComponentType.Box2DBody;

    switch (packet.componentId) {
      case 1: // Box2DBody - hardcoded since ComponentType.Box2DBody isn't working
      case ComponentType.Box2DBody:
        // Read Box2DBody component matching server struct layout (Pack=1, no padding):
        // long ChangedTick (8), B2BodyId (8), bool IsStatic (1), uint Color (4),
        // float Density (4), int Sides (4), Vector2 LastPosition (8), float LastRotation (4)
        const _bodyChangedTick = reader.i64();
        // Skip B2BodyId (8 bytes - two int32s)
        reader.Skip(8);
        const isStatic = reader.i8() !== 0;
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

        // Disable verbose logging to prevent console spam
        // if (isPlayerBox2DBody) {
        //   console.log(
        //     `[CLIENT PLAYER] Box2DBody component received for ${packet.entityId} at pos=(${positionX}, ${positionY}), vel=(${velocityX}, ${velocityY})`,
        //   );
        // }

        // Set up or update physics component with actual position/velocity data
        if (!entity.has(PhysicsComponent)) {
          // All entities use 1x1 unit size to match server
          const physics = new PhysicsComponent(entity.id, {
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
          const physics = entity.get(PhysicsComponent);
          if (physics) {
            const oldPos = { x: physics.position.x, y: physics.position.y };
            physics.position = { x: positionX, y: positionY };
            physics.linearVelocity = { x: velocityX, y: velocityY };
            physics.setRotation(rotation);
          }
        }

        // Set up or update network component with position/velocity data
        if (!entity.has(NetworkComponent)) {
          const network = new NetworkComponent(entity.id, {
            serverId: packet.entityId,
            isLocallyControlled: isLocalPlayer,
            serverPosition: { x: positionX, y: positionY },
            serverVelocity: { x: velocityX, y: velocityY },
            serverRotation: rotation,
          });
          entity.set(network);
        } else {
          // Update existing network component with new server state
          const network = entity.get(NetworkComponent);
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
        const _gravityChangedTick = reader.i64();
        const strength = reader.f32();
        const radius = reader.f32();

        entity.set(new GravityComponent(packet.entityId, strength, radius));
        break;

      case ComponentType.Health:
        const _healthChangedTick = reader.i64();
        const health = reader.f32();
        const maxHealth = reader.f32();

        entity.set(new HealthComponent(packet.entityId, maxHealth, health, 0));
        break;

      case ComponentType.Energy:
        const _energyChangedTick = reader.i64();
        const _dischargeRateAcc = reader.f32();
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
        // Server struct (Pack=1): ChangedTick (8), PowerOn (1), LastPowerOn (1),
        // Charge (4), MaxCharge (4), PowerUse (4), PowerUseRecharge (4),
        // Radius (4), MinRadius (4), TargetRadius (4), RechargeRate (4),
        // RechargeDelayTicks (8), LastDamageTimeTicks (8)
        const _shieldChangedTick = reader.i64();
        const _powerOn = reader.i8() !== 0;
        const _lastPowerOn = reader.i8() !== 0;
        // NO padding with Pack=1
        const charge = reader.f32();
        const maxCharge = reader.f32();
        const powerUse = reader.f32();
        const _powerUseRecharge = reader.f32();
        const shieldRadius = reader.f32();
        const _minRadius = reader.f32();
        const _targetRadius = reader.f32();
        const rechargeRate = reader.f32();
        const _rechargeDelayTicks = reader.i64();
        const _lastDamageTimeTicks = reader.i64();

        entity.set(
          new ShieldComponent(packet.entityId, {
            charge,
            maxCharge,
            powerUse,
            radius: shieldRadius,
            rechargeRate,
          }),
        );
        break;

      case ComponentType.Engine:
        // Server struct (Pack=1): ChangedTick (8), PowerUse (4), Throttle (4),
        // MaxThrustNewtons (4), RCS (1), Rotation (4)
        const _engineChangedTick = reader.i64();
        const enginePowerUse = reader.f32();
        const throttle = reader.f32();
        const maxThrustNewtons = reader.f32();
        const rcs = reader.i8() !== 0;
        // NO padding with Pack=1
        const engineRotation = reader.f32();

        entity.set(
          new EngineComponent(packet.entityId, {
            maxPropulsion: maxThrustNewtons,
            powerUse: enginePowerUse,
            throttle,
            rcs,
            rotation: engineRotation,
          }),
        );
        break;

      case ComponentType.Level:
        const _levelChangedTick = reader.i64();
        const level = reader.i32();
        const expToNextLevel = reader.i32();
        const experience = reader.i32();

        break;

      case ComponentType.Inventory:
        const _inventoryChangedTick = reader.i64();
        const totalCapacity = reader.i32();
        const triangles = reader.i32();
        const squares = reader.i32();
        const pentagons = reader.i32();

        break;

      case ComponentType.NameTag:
        const _nameChangedTick = reader.i64();
        const nameBytes = new Uint8Array(64);
        for (let i = 0; i < 64; i++) {
          nameBytes[i] = reader.i8();
        }
        const nullIndex = nameBytes.indexOf(0);
        const nameLength = nullIndex >= 0 ? nullIndex : 64;
        const nameString = new TextDecoder().decode(
          nameBytes.subarray(0, nameLength),
        );

        // console.log(`NameTag for ${packet.entityId}: "${nameString}"`);

        const nameEvent = new CustomEvent("player-name-update", {
          detail: { entityId: packet.entityId, name: nameString },
        });
        window.dispatchEvent(nameEvent);
        break;

      case ComponentType.DeathTag:
        const _deathChangedTick = reader.i64();
        const killerGuid = reader.Guid();

        // console.log(`DeathTag for ${packet.entityId}: killed by ${killerGuid}`);

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
          }
        }

        const deathEvent = new CustomEvent("entity-death", {
          detail: { entityId: packet.entityId, killerId: killerGuid },
        });
        window.dispatchEvent(deathEvent);
        break;

      case ComponentType.ShipPart:
        const _partChangedTick = reader.i64();
        const gridX = reader.i8();
        const gridY = reader.i8();
        const partType = reader.i8();
        const partShape = reader.i8();
        const partRotation = reader.i8();

        // Create ship part entity with component
        let partEntity = World.getEntity(packet.entityId);
        if (!partEntity) {
          partEntity = World.createEntity(EntityType.ShipPart, packet.entityId);
        }

        const shipPartComponent = new ShipPartComponent(packet.entityId, {
          gridX,
          gridY,
          type: partType,
          shape: partShape,
          rotation: partRotation,
        });
        partEntity.set(shipPartComponent);

        // If this entity already has a ParentChild component, trigger sync
        const existingParentChild = partEntity.get(ParentChildComponent);
        if (existingParentChild) {
          const syncEvent = new CustomEvent("parent-child-update", {
            detail: {
              childId: packet.entityId,
              parentId: existingParentChild.parentId,
            },
          });
          window.dispatchEvent(syncEvent);
        }
        break;

      case ComponentType.ParentChild:
        const _parentChangedTick = reader.i64();
        const parentId = reader.Guid();

        // Create parent-child entity with component
        let childEntity = World.getEntity(packet.entityId);
        if (!childEntity) {
          childEntity = World.createEntity(
            EntityType.ShipPart,
            packet.entityId,
          );
        }

        const parentChildComponent = new ParentChildComponent(
          packet.entityId,
          parentId,
        );
        childEntity.set(parentChildComponent);

        // Store parent-child relationship - can be used by rendering system
        const childEvent = new CustomEvent("parent-child-update", {
          detail: { childId: packet.entityId, parentId: parentId },
        });
        window.dispatchEvent(childEvent);
        break;

      case ComponentType.Color:
        const _colorChangedTick = reader.i64();
        const colorValue = reader.u32();

        entity.set(new ColorComponent(packet.entityId, colorValue));
        break;

      case ComponentType.Lifetime:
        const _lifetimeChangedTick = reader.i64();
        const lifetimeSeconds = reader.f32();

        // console.log(
        //   `LifetimeComponent received for entity ${packet.entityId}: ${lifetimeSeconds} seconds`,
        // );
        entity.set(new LifetimeComponent(packet.entityId, lifetimeSeconds));
        break;

      case ComponentType.HealthRegen:
        // HealthRegenComponent: long ChangedTick (8), float PassiveHealPerSec (4)
        const _healthRegenChangedTick = reader.i64();
        const _passiveHealPerSec = reader.f32();
        // Client doesn't need to track health regen - server handles it
        break;

      case ComponentType.Weapon:
        // WeaponComponent: long ChangedTick (8), NTT Owner (16), bool Fire (1),
        // TimeSpan Frequency (8), TimeSpan LastShot (8), ushort BulletDamage (2),
        // byte BulletCount (1), byte BulletSize (1), ushort BulletSpeed (2),
        // float PowerUse (4), Vector2 Direction (8)
        const _weaponChangedTick = reader.i64();
        reader.Skip(16); // NTT Owner
        const _fire = reader.i8();
        reader.Skip(8); // TimeSpan Frequency
        reader.Skip(8); // TimeSpan LastShot
        const _bulletDamage = reader.u16();
        const _bulletCount = reader.i8(); // byte (u8)
        const _bulletSize = reader.i8(); // byte (u8)
        const _bulletSpeed = reader.u16();
        const _powerUse = reader.f32();
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
