export interface EntitySnapshot {
  id: number;
  timestamp: number;
  position: { x: number; y: number };
  velocity: { x: number; y: number };
  rotation: number;
  health?: number;
  energy?: number;
  size?: number;
}

export interface WorldSnapshot {
  timestamp: number;
  entities: Map<number, EntitySnapshot>;
}

export class SnapshotBuffer {
  private snapshots: WorldSnapshot[] = [];
  private maxSnapshots = 60;
  private interpolationDelay = 100; // ms

  addSnapshot(snapshot: WorldSnapshot): void {
    this.snapshots.push(snapshot);
    
    // Keep buffer size limited
    while (this.snapshots.length > this.maxSnapshots) {
      this.snapshots.shift();
    }
  }

  getInterpolatedState(currentTime: number): WorldSnapshot | null {
    const renderTime = currentTime - this.interpolationDelay;
    
    // Find the two snapshots to interpolate between
    let older: WorldSnapshot | null = null;
    let newer: WorldSnapshot | null = null;
    
    for (let i = 0; i < this.snapshots.length - 1; i++) {
      if (this.snapshots[i].timestamp <= renderTime && 
          this.snapshots[i + 1].timestamp >= renderTime) {
        older = this.snapshots[i];
        newer = this.snapshots[i + 1];
        break;
      }
    }
    
    if (!older || !newer) {
      return this.snapshots[this.snapshots.length - 1] || null;
    }
    
    // Calculate interpolation factor
    const total = newer.timestamp - older.timestamp;
    const partial = renderTime - older.timestamp;
    const t = total > 0 ? partial / total : 0;
    
    // Interpolate entities
    const interpolated: WorldSnapshot = {
      timestamp: renderTime,
      entities: new Map()
    };
    
    newer.entities.forEach((newerEntity, id) => {
      const olderEntity = older.entities.get(id);
      if (!olderEntity) {
        interpolated.entities.set(id, newerEntity);
        return;
      }
      
      interpolated.entities.set(id, {
        id,
        timestamp: renderTime,
        position: {
          x: this.lerp(olderEntity.position.x, newerEntity.position.x, t),
          y: this.lerp(olderEntity.position.y, newerEntity.position.y, t)
        },
        velocity: {
          x: this.lerp(olderEntity.velocity.x, newerEntity.velocity.x, t),
          y: this.lerp(olderEntity.velocity.y, newerEntity.velocity.y, t)
        },
        rotation: this.lerpAngle(olderEntity.rotation, newerEntity.rotation, t),
        health: newerEntity.health,
        energy: newerEntity.energy,
        size: newerEntity.size
      });
    });
    
    return interpolated;
  }
  
  private lerp(a: number, b: number, t: number): number {
    return a + (b - a) * t;
  }
  
  private lerpAngle(a: number, b: number, t: number): number {
    let diff = b - a;
    while (diff > Math.PI) diff -= 2 * Math.PI;
    while (diff < -Math.PI) diff += 2 * Math.PI;
    return a + diff * t;
  }
  
  getLatestSnapshot(): WorldSnapshot | null {
    return this.snapshots[this.snapshots.length - 1] || null;
  }
  
  clear(): void {
    this.snapshots = [];
  }
}