import { Component } from "../core/Component";
import { Entity } from "../core/Entity";

export interface AABB {
  x: number;
  y: number;
  width: number;
  height: number;
}

export interface AABBConfig {
  aabb?: AABB;
}

export class AABBComponent extends Component {
  aabb: AABB;
  potentialCollisions: Entity[];

  constructor(entityId: number, config: AABBConfig = {}) {
    super(entityId);

    this.aabb = config.aabb
      ? { ...config.aabb }
      : { x: 0, y: 0, width: 0, height: 0 };
    this.potentialCollisions = [];
  }

  updateAABB(centerX: number, centerY: number, radius: number): void {
    this.aabb.x = centerX - radius;
    this.aabb.y = centerY - radius;
    this.aabb.width = radius * 2;
    this.aabb.height = radius * 2;
    this.markChanged();
  }

  updateAABBRect(centerX: number, centerY: number, width: number, height: number): void {
    this.aabb.x = centerX - width / 2;
    this.aabb.y = centerY - height / 2;
    this.aabb.width = width;
    this.aabb.height = height;
    this.markChanged();
  }

  intersects(other: AABB): boolean {
    return !(
      this.aabb.x > other.x + other.width ||
      this.aabb.x + this.aabb.width < other.x ||
      this.aabb.y > other.y + other.height ||
      this.aabb.y + this.aabb.height < other.y
    );
  }
}
