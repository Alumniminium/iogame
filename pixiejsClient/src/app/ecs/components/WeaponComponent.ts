import { Component, component, serverField } from "../core/Component";
import { ServerComponentType } from "../../enums/ComponentIds";
import type { Vector2 } from "../core/types";

@component(ServerComponentType.Weapon)
export class WeaponComponent extends Component {
  // changedTick is inherited from Component base class
  @serverField(1, "guid") owner: string;
  @serverField(2, "bool") fire: boolean;
  @serverField(3, "i64") frequency: bigint; // TimeSpan as ticks
  @serverField(4, "i64") lastShot: bigint; // TimeSpan as ticks
  @serverField(5, "u16") bulletDamage: number;
  @serverField(6, "u8") bulletCount: number;
  @serverField(7, "u8") bulletSize: number;
  @serverField(8, "u16") bulletSpeed: number;
  @serverField(9, "f32") powerUse: number;
  @serverField(10, "vector2") direction: Vector2;

  constructor(
    entityId: string,
    owner: string = "",
    fire: boolean = false,
    frequency: bigint = 0n,
    lastShot: bigint = 0n,
    bulletDamage: number = 0,
    bulletCount: number = 1,
    bulletSize: number = 5,
    bulletSpeed: number = 50,
    powerUse: number = 5.0,
    direction: Vector2 = { x: 1, y: 0 },
  ) {
    super(entityId);
    this.owner = owner;
    this.fire = fire;
    this.frequency = frequency;
    this.lastShot = lastShot;
    this.bulletDamage = bulletDamage;
    this.bulletCount = bulletCount;
    this.bulletSize = bulletSize;
    this.bulletSpeed = bulletSpeed;
    this.powerUse = powerUse;
    this.direction = direction;
  }
}
