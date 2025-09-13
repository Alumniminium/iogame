import { System } from '../core/System';
import { Entity } from '../core/Entity';
import { PhysicsComponent } from '../components/PhysicsComponent';
import { NetworkComponent } from '../components/NetworkComponent';
import { RenderComponent } from '../components/RenderComponent';
import { Vector2 } from '../core/types';

export interface Camera {
  x: number;
  y: number;
  zoom: number;
}

export class ClientRenderSystem extends System {
  readonly componentTypes = [PhysicsComponent]; // Render all entities with physics

  private canvas: HTMLCanvasElement;
  private ctx: CanvasRenderingContext2D;
  private camera: Camera;
  private interpolationAlpha = 0;
  private mapWidth = 10000;
  private mapHeight = 10000;
  private gridSize = 25;
  private followEntityId: number | null = null;

  constructor(canvas: HTMLCanvasElement) {
    super();
    this.canvas = canvas;
    this.ctx = canvas.getContext('2d')!;
    this.camera = { x: 0, y: 0, zoom: 1 };

    // Handle canvas resize
    window.addEventListener('resize', () => this.resizeCanvas());
    this.resizeCanvas();
  }

  initialize(): void {
    console.log('ClientRenderSystem initialized');
  }

  private resizeCanvas(): void {
    // Set canvas size to match display size
    this.canvas.width = this.canvas.offsetWidth;
    this.canvas.height = this.canvas.offsetHeight;

    // Reset any transforms
    this.ctx.setTransform(1, 0, 0, 1, 0, 0);
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

  update(deltaTime: number): void {
    // Update interpolation alpha for smooth rendering
    this.interpolationAlpha = Math.min(1, this.interpolationAlpha + deltaTime * 60);

    // Update camera to follow entity if set
    if (this.followEntityId) {
      const entity = this.getEntity(this.followEntityId);
      if (entity) {
        const physics = entity.getComponent(PhysicsComponent);
        if (physics) {
          this.camera.x = physics.position.x;
          this.camera.y = physics.position.y;
          // console.log(`📷 Camera following entity ${this.followEntityId} at (${physics.position.x.toFixed(1)}, ${physics.position.y.toFixed(1)})`);
        } else {
          console.warn(`📷 Entity ${this.followEntityId} has no physics component`);
        }
      } else {
        // Fallback to global position if ECS entity doesn't exist yet
        const globalPos = (window as any).localPlayerPosition;
        if (globalPos) {
          this.camera.x = globalPos.x;
          this.camera.y = globalPos.y;
          // console.log(`📷 Camera fallback to global position (${globalPos.x.toFixed(1)}, ${globalPos.y.toFixed(1)})`);
        }
      }
    }
  }

  protected updateEntity(entity: Entity, deltaTime: number): void {
    // ClientRenderSystem doesn't process individual entities in updateEntity
    // Instead it renders all at once in the render() method
  }

  render(): void {
    // Get all entities with physics components for rendering
    const entities = this.queryEntities([PhysicsComponent]);

    // Update zoom based on view distance (could be configurable)
    const viewDistance = 300;
    const screenSize = Math.min(this.canvas.width, this.canvas.height);
    this.camera.zoom = screenSize / (viewDistance * 2);

    // Clear canvas
    this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);

    // Save context state
    this.ctx.save();

    // Apply camera transform - center the camera position on screen
    this.ctx.translate(this.canvas.width / 2, this.canvas.height / 2);
    this.ctx.scale(this.camera.zoom, this.camera.zoom);
    this.ctx.translate(-this.camera.x, -this.camera.y);

    // Draw background
    this.drawBackground();

    // Draw grid
    this.drawGrid();

    // Draw camera center crosshair for debugging
    this.drawCameraCrosshair();

    // Render all entities
    entities.forEach(entity => {
      this.renderEntity(entity);
    });

    // Restore context state
    this.ctx.restore();
  }

  followEntity(entity: Entity): void {
    this.followEntityId = entity.id;
  }

  unfollowEntity(): void {
    this.followEntityId = null;
  }

  setCamera(position: Vector2, zoom: number = 1): void {
    this.camera.x = position.x;
    this.camera.y = position.y;
    this.camera.zoom = zoom;
  }

  getCamera(): Camera {
    return { ...this.camera };
  }

  private renderEntity(entity: Entity): void {
    const physics = entity.getComponent(PhysicsComponent)!
    const network = entity.getComponent(NetworkComponent);
    const render = entity.getComponent(RenderComponent);

    this.ctx.save();
    this.ctx.translate(physics.position.x, physics.position.y);
    this.ctx.rotate(physics.rotation);

    // Use physics size, with minimum visibility
    const size = Math.max(physics.size, 16);
    const sides = render?.sides || 3; // Get sides from render component

    // Highlight local player
    const isLocal = network?.isLocallyControlled || false;

    // Draw polygon based on number of sides
    this.drawPolygon(size, sides, isLocal);

    // Draw gun barrel for local player
    if (isLocal) {
      this.drawGunBarrel(size);
    }

    this.ctx.restore();
  }


  private drawPolygon(size: number, sides: number, isLocal: boolean = false): void {
    // Color based on sides and local status
    const colors = [
      '#ffffff', // 0 sides - circles (white)
      '#ff0000', // 1 side (shouldn't happen)
      '#ff00ff', // 2 sides - rings (magenta)
      '#ff8800', // 3 sides - triangles (orange)
      '#ffff00', // 4 sides - squares (yellow)
      '#00ff00', // 5 sides - pentagons (green)
      '#00ffff', // 6 sides - hexagons (cyan)
      '#0088ff', // 7 sides - heptagons (blue)
      '#8800ff'  // 8 sides - octagons (purple)
    ];

    const baseColor = colors[sides] || '#808080';
    this.ctx.fillStyle = isLocal ? '#ffffff' : baseColor;
    this.ctx.strokeStyle = isLocal ? '#cccccc' : '#ffffff';
    this.ctx.lineWidth = isLocal ? 3 : 1;

    if (sides < 3) {
      // Fallback to circle for invalid sides
      this.ctx.beginPath();
      this.ctx.arc(0, 0, size / 2, 0, Math.PI * 2);
    } else {
      // Draw regular polygon
      this.ctx.beginPath();
      for (let i = 0; i < sides; i++) {
        const angle = (Math.PI * 2 / sides) * i - Math.PI / 2;
        const x = Math.cos(angle) * size / 2;
        const y = Math.sin(angle) * size / 2;

        if (i === 0) {
          this.ctx.moveTo(x, y);
        } else {
          this.ctx.lineTo(x, y);
        }
      }
      this.ctx.closePath();
    }

    this.ctx.fill();
    this.ctx.stroke();
  }

  private drawBackground(): void {
    this.ctx.fillStyle = '#0a0a0a';

    // Calculate visible area
    const viewLeft = this.camera.x - this.canvas.width / (2 * this.camera.zoom);
    const viewRight = this.camera.x + this.canvas.width / (2 * this.camera.zoom);
    const viewTop = this.camera.y - this.canvas.height / (2 * this.camera.zoom);
    const viewBottom = this.camera.y + this.canvas.height / (2 * this.camera.zoom);

    // Fill visible area
    this.ctx.fillRect(viewLeft, viewTop, viewRight - viewLeft, viewBottom - viewTop);
  }

  private drawGrid(): void {
    this.ctx.save();
    this.ctx.globalAlpha = 0.1;
    this.ctx.strokeStyle = '#c7c7c7';
    this.ctx.lineWidth = 0.5;

    // Calculate visible area
    const viewLeft = this.camera.x - this.canvas.width / (2 * this.camera.zoom);
    const viewRight = this.camera.x + this.canvas.width / (2 * this.camera.zoom);
    const viewTop = this.camera.y - this.canvas.height / (2 * this.camera.zoom);
    const viewBottom = this.camera.y + this.canvas.height / (2 * this.camera.zoom);

    // Draw vertical lines
    this.ctx.beginPath();
    for (let x = Math.floor(viewLeft / this.gridSize) * this.gridSize; x <= viewRight; x += this.gridSize) {
      if (x >= 0 && x <= this.mapWidth) {
        this.ctx.moveTo(x, Math.max(0, viewTop));
        this.ctx.lineTo(x, Math.min(this.mapHeight, viewBottom));
      }
    }

    // Draw horizontal lines
    for (let y = Math.floor(viewTop / this.gridSize) * this.gridSize; y <= viewBottom; y += this.gridSize) {
      if (y >= 0 && y <= this.mapHeight) {
        this.ctx.moveTo(Math.max(0, viewLeft), y);
        this.ctx.lineTo(Math.min(this.mapWidth, viewRight), y);
      }
    }

    this.ctx.stroke();
    this.ctx.restore();
  }

  private drawCameraCrosshair(): void {
    this.ctx.save();
    this.ctx.strokeStyle = '#ff0000';
    this.ctx.lineWidth = 2;
    this.ctx.globalAlpha = 0.8;

    // Draw crosshair at camera center (which should be the player position)
    this.ctx.beginPath();
    // Horizontal line
    this.ctx.moveTo(this.camera.x - 50, this.camera.y);
    this.ctx.lineTo(this.camera.x + 50, this.camera.y);
    // Vertical line
    this.ctx.moveTo(this.camera.x, this.camera.y - 50);
    this.ctx.lineTo(this.camera.x, this.camera.y + 50);
    this.ctx.stroke();

    this.ctx.restore();
  }

  private drawGunBarrel(playerSize: number): void {
    const barrelLength = playerSize * 0.8;
    const barrelWidth = playerSize * 0.15;

    this.ctx.fillStyle = '#cccccc';
    this.ctx.strokeStyle = '#999999';
    this.ctx.lineWidth = 1;

    // Draw barrel as rectangle extending forward (rotation is already applied)
    // The barrel points in the direction the entity is facing (0 degrees is right)
    this.ctx.fillRect(0, -barrelWidth / 2, barrelLength, barrelWidth);
    this.ctx.strokeRect(0, -barrelWidth / 2, barrelLength, barrelWidth);
  }

}