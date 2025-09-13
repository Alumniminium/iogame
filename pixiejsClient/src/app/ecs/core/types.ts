export enum EntityType {
  Static = 0,
  Passive = 1,
  Pickable = 2,
  Projectile = 3,
  Npc = 4,
  Player = 5,
  Debug = 6,
}

export interface Vector2 {
  x: number;
  y: number;
}
