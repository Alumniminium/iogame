/**
 * Entity type flags used for categorizing entities in the ECS world.
 * Can be combined using bitwise operations for entity filtering.
 */
export enum EntityType {
  Static = 1,
  Passive = 2,
  Pickable = 4,
  Projectile = 8,
  Npc = 16,
  Player = 32,
  Debug = 64,
  ShipPart = 128,
}

/**
 * 2D vector for position, velocity, and other spatial calculations
 */
export interface Vector2 {
  x: number;
  y: number;
}
