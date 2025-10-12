import { Component, component, serverField } from "../core/Component";
import { ServerComponentType } from "../../enums/ComponentIds";
import type { Vector2 } from "../core/types";
import { NTT } from "../core/NTT";

@component(ServerComponentType.Weapon)
export class WeaponComponent extends Component {
  @serverField(1, "guid") owner: string;
  @serverField(2, "bool") fire: boolean;
  @serverField(3, "f32") frequency: number;
  @serverField(4, "i64") lastShot: bigint;
  @serverField(5, "u16") bulletDamage: number;
  @serverField(6, "u8") bulletCount: number;
  @serverField(7, "u8") bulletSize: number;
  @serverField(8, "u16") bulletSpeed: number;
  @serverField(9, "f32") powerUse: number;
  @serverField(10, "vector2") direction: Vector2;

  constructor(
    ntt: NTT,
    owner: string = "",
    fire: boolean = false,
    frequency: number = 0,
    lastShot: bigint = 0n,
    bulletDamage: number = 0,
    bulletCount: number = 1,
    bulletSize: number = 5,
    bulletSpeed: number = 50,
    powerUse: number = 5.0,
    direction: Vector2 = { x: 1, y: 0 },
  ) {
    super(ntt);
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
