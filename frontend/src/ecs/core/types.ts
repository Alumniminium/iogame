export enum EntityType {
  Player = 'player',
  Enemy = 'enemy',
  Projectile = 'projectile',
  Resource = 'resource'
}

export interface Vector2 {
  x: number;
  y: number;
}