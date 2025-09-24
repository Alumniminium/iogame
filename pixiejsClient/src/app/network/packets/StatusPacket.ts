import { EvPacketReader } from "../EvPacketReader";
import { PacketHeader } from "../PacketHeader";
import { World } from "../../ecs/core/World";
import { EntityType } from "../../ecs/core/types";
import { HealthComponent } from "../../ecs/components/HealthComponent";
import { EnergyComponent } from "../../ecs/components/EnergyComponent";
import { BatteryComponent } from "../../ecs/components/BatteryComponent";
import { ShieldComponent } from "../../ecs/components/ShieldComponent";

export class StatusPacket {
  header: PacketHeader;
  entityId: string;
  value: number;
  statusType: number;

  constructor(
    header: PacketHeader,
    entityId: string,
    value: number,
    statusType: number,
  ) {
    this.header = header;
    this.entityId = entityId;
    this.value = value;
    this.statusType = statusType;
  }

  static fromBuffer(buffer: ArrayBuffer): StatusPacket {
    const reader = new EvPacketReader(buffer);
    const header = reader.Header();
    const entityId = reader.Guid();
    const value = reader.f64();
    const statusType = reader.i8();

    return new StatusPacket(header, entityId, value, statusType);
  }

  static handle(buffer: ArrayBuffer): void {
    const packet = StatusPacket.fromBuffer(buffer);

    let entity = World.getEntity(packet.entityId);
    if (!entity) {
      if (packet.statusType === 0 && packet.value === 1) {
        entity = World.createEntity(EntityType.Player, packet.entityId);
      } else {
        return;
      }
    }
    switch (packet.statusType) {
      case 0: // Alive - handle despawn when value is 0
        if (packet.value === 0) {
          const localPlayerId = (window as any).localPlayerId;
          if (packet.entityId === localPlayerId) {
            return;
          }

          World.destroyEntity(entity);
          return; // Don't process further status updates for destroyed entity
        }
        break;
      case 1: // Health
        let health = entity.get(HealthComponent);
        if (!health) {
          health = new HealthComponent(entity.id, { max: 100, current: 100 });
          entity.set(health);
        }
        health.current = packet.value;
        health.isDead = health.current <= 0;
        break;
      case 2: // MaxHealth
        let maxHealth = entity.get(HealthComponent);
        if (!maxHealth) {
          maxHealth = new HealthComponent(entity.id, {
            max: 100,
            current: 100,
          });
          entity.set(maxHealth);
        }
        maxHealth.max = packet.value;
        maxHealth.isDead = maxHealth.current <= 0;
        break;
      case 6: // Energy
        let energy = entity.get(EnergyComponent);
        if (!energy) {
          energy = new EnergyComponent(entity.id, { batteryCapacity: 100 });
          entity.set(energy);
        }
        energy.availableCharge = packet.value;
        energy.markChanged();
        break;
      case 7: // MaxEnergy
        let maxEnergy = entity.get(EnergyComponent);
        if (!maxEnergy) {
          maxEnergy = new EnergyComponent(entity.id, { batteryCapacity: 100 });
          entity.set(maxEnergy);
        }
        maxEnergy.batteryCapacity = packet.value;
        maxEnergy.markChanged();
        break;
      case 11: // BatteryCharge
        let battery = entity.get(BatteryComponent);
        if (!battery) {
          battery = new BatteryComponent(entity.id, {});
          entity.set(battery);
        }
        battery.currentCharge = packet.value;
        break;
      case 10: // BatteryCapacity
        let batteryCapacity = entity.get(BatteryComponent);
        if (!batteryCapacity) {
          batteryCapacity = new BatteryComponent(entity.id, {});
          entity.set(batteryCapacity);
        }
        batteryCapacity.capacity = packet.value;
        break;
      case 20: // ShieldCharge
        let shield = entity.get(ShieldComponent);
        if (!shield) {
          shield = new ShieldComponent(entity.id, { maxCharge: 100 });
          entity.set(shield);
        }
        shield.charge = packet.value;
        const chargePercent = shield.charge / shield.maxCharge;
        shield.radius = Math.max(
          shield.minRadius,
          shield.targetRadius * chargePercent,
        );
        shield.powerOn = shield.charge > 0;
        shield.markChanged();
        break;
      case 21: // ShieldMaxCharge
        let shieldMax = entity.get(ShieldComponent);
        if (!shieldMax) {
          shieldMax = new ShieldComponent(entity.id, { maxCharge: 100 });
          entity.set(shieldMax);
        }
        shieldMax.maxCharge = packet.value;
        shieldMax.markChanged();
        break;
      case 25: // ShieldRadius
        let shieldRadius = entity.get(ShieldComponent);
        if (!shieldRadius) {
          shieldRadius = new ShieldComponent(entity.id, { maxCharge: 100 });
          entity.set(shieldRadius);
        }
        shieldRadius.radius = packet.value;
        shieldRadius.markChanged();
        break;
    }
  }
}
