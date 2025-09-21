export enum EntityType {
  Static = 1,
  Passive = 2,
  Pickable = 4,
  Projectile = 8,
  Npc = 16,
  Player = 32,
  Debug = 64,
}

export interface Vector2 {
  x: number;
  y: number;
}
