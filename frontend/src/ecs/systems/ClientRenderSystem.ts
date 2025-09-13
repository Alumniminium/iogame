import { System } from '../core/System';
import { Entity } from '../core/Entity';
import { PhysicsComponent } from '../components/PhysicsComponent';
import { NetworkComponent } from '../components/NetworkComponent';
import { HealthComponent } from '../components/HealthComponent';
import { EnergyComponent } from '../components/EnergyComponent';

export class ClientRenderSystem extends System {
  private canvas: HTMLCanvasElement;
  private ctx: CanvasRenderingContext2D;
  private camera: { x: number; y: number; zoom: number };
  private interpolationAlpha = 0;
  private mapWidth = 10000;
  private mapHeight = 10000;
  private gridSize = 25;

  constructor(canvas: HTMLCanvasElement) {
    super();
    this.canvas = canvas;
    this.ctx = canvas.getContext('2d')!;
    this.camera = { x: 0, y: 0, zoom: 1 };

    // Handle canvas resize
    window.addEventListener('resize', () => this.resizeCanvas());
    this.resizeCanvas();
  }

  private resizeCanvas(): void {
    // Set canvas size to match display size
    this.canvas.width = this.canvas.offsetWidth;
    this.canvas.height = this.canvas.offsetHeight;

    // Reset any transforms
    this.ctx.setTransform(1, 0, 0, 1, 0, 0);
  }

  setCamera(x: number, y: number, zoom: number = 1): void {
    this.camera = { x, y, zoom };
  }

  followEntity(entity: Entity): void {
    const physics = entity.getComponent<PhysicsComponent>('physics');
    if (physics) {
      // Smooth camera follow with interpolation
      const network = entity.getComponent<NetworkComponent>('network');
      if (network && network.isLocallyControlled) {
        // For local player, use actual position
        this.camera.x = physics.position.x;
        this.camera.y = physics.position.y;
      } else {
        // For remote entities, interpolate
        const lerpedX = this.lerp(physics.lastPosition.x, physics.position.x, this.interpolationAlpha);
        const lerpedY = this.lerp(physics.lastPosition.y, physics.position.y, this.interpolationAlpha);
        this.camera.x = lerpedX;
        this.camera.y = lerpedY;
      }
    }
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
  }

  render(entities: Entity[]): void {
    // Update camera to follow local player
    const localPlayerPos = (window as any).localPlayerPosition;
    if (localPlayerPos) {
      this.camera.x = localPlayerPos.x;
      this.camera.y = localPlayerPos.y;
    }

    // Update zoom based on view distance
    const viewDistance = (window as any).viewDistance || 300;
    // Calculate zoom so that viewDistance units fit in the smaller screen dimension
    const screenSize = Math.min(this.canvas.width, this.canvas.height);
    this.camera.zoom = screenSize / (viewDistance * 2); // *2 because viewDistance is radius

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

    // Get entities from global store (temporary solution)
    const gameEntities = (window as any).gameEntities as Map<number, any>;
    if (gameEntities && gameEntities.size > 0) {
      // Find local player for camera debugging
      let localPlayerEntity = null;
      gameEntities.forEach((entity) => {
        if (entity.isLocal) {
          localPlayerEntity = entity;
        }
        this.renderNetworkEntity(entity);
      });

      // Debug camera occasionally
      if (Math.random() < 0.01) { // ~1% chance per frame
        console.log(`📷 CAMERA: (${this.camera.x.toFixed(1)}, ${this.camera.y.toFixed(1)}) zoom: ${this.camera.zoom.toFixed(2)}`);
        if (localPlayerEntity) {
          console.log(`👤 LOCAL PLAYER: (${localPlayerEntity.position.x.toFixed(1)}, ${localPlayerEntity.position.y.toFixed(1)})`);
        }
        const viewDistance = (window as any).viewDistance || 300;
        console.log(`🔍 VIEW: distance=${viewDistance}, screen=${Math.min(this.canvas.width, this.canvas.height)}, zoom=${this.camera.zoom.toFixed(3)}`);
      }
    }

    // Restore context state
    this.ctx.restore();
  }

  private renderNetworkEntity(entity: any): void {
    this.ctx.save();
    this.ctx.translate(entity.position.x, entity.position.y);
    this.ctx.rotate(entity.rotation);

    // Use actual size from base resources, but ensure minimum visibility
    const size = Math.max(entity.size || 16, 16);
    const sides = entity.sides || 3;

    // Highlight local player
    const isLocal = entity.isLocal;

    // Draw polygon based on number of sides
    this.drawPolygon(size, sides, isLocal);

    this.ctx.restore();
  }

  private drawCircle(size: number, isLocal: boolean = false): void {
    this.ctx.fillStyle = isLocal ? '#ffff00' : '#00ff00';
    this.ctx.strokeStyle = isLocal ? '#ffaa00' : '#00aa00';
    this.ctx.lineWidth = isLocal ? 4 : 2;
    this.ctx.beginPath();
    this.ctx.arc(0, 0, size / 2, 0, Math.PI * 2);
    this.ctx.fill();
    this.ctx.stroke();
  }

  private drawRectangle(size: number, isLocal: boolean = false): void {
    this.ctx.fillStyle = isLocal ? '#ff00ff' : '#0088ff';
    this.ctx.strokeStyle = isLocal ? '#aa00aa' : '#0066aa';
    this.ctx.lineWidth = isLocal ? 4 : 2;
    this.ctx.fillRect(-size / 2, -size / 2, size, size);
    this.ctx.strokeRect(-size / 2, -size / 2, size, size);
  }

  private drawTriangle(size: number, isLocal: boolean = false): void {
    this.ctx.fillStyle = isLocal ? '#ff0000' : '#ff8800';
    this.ctx.strokeStyle = isLocal ? '#aa0000' : '#aa6600';
    this.ctx.lineWidth = isLocal ? 4 : 2;

    this.ctx.beginPath();
    this.ctx.moveTo(size / 2, 0);
    this.ctx.lineTo(-size / 2, -size / 3);
    this.ctx.lineTo(-size / 2, size / 3);
    this.ctx.closePath();
    this.ctx.fill();
    this.ctx.stroke();
  }

  private drawPolygon(size: number, sides: number, isLocal: boolean = false): void {
    // Color based on sides and local status
    const colors = [
      '#808080', // 0 sides (shouldn't happen)
      '#ff0000', // 1 side (shouldn't happen)
      '#ff00ff', // 2 sides (shouldn't happen)
      '#ff8800', // 3 sides - triangles (orange)
      '#ffff00', // 4 sides - squares (yellow)
      '#00ff00', // 5 sides - pentagons (green)
      '#00ffff', // 6 sides - hexagons (cyan)
      '#0088ff', // 7 sides - heptagons (blue)
      '#8800ff'  // 8 sides - octagons (purple)
    ];

    this.ctx.fillStyle = isLocal ? '#ff0000' : (colors[sides] || '#808080');
    this.ctx.strokeStyle = isLocal ? '#aa0000' : '#ffffff';
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

  private renderEntity(entity: Entity): void {
    const physics = entity.getComponent<PhysicsComponent>('physics');
    if (!physics) return;

    const network = entity.getComponent<NetworkComponent>('network');

    // Calculate interpolated position
    let x = physics.position.x;
    let y = physics.position.y;
    let rotation = physics.rotation;

    if (network && !network.isLocallyControlled) {
      // Interpolate remote entities
      x = this.lerp(physics.lastPosition.x, physics.position.x, this.interpolationAlpha);
      y = this.lerp(physics.lastPosition.y, physics.position.y, this.interpolationAlpha);
      rotation = this.lerpAngle(physics.lastRotation, physics.rotation, this.interpolationAlpha);
    }

    // Draw entity based on type
    this.ctx.save();
    this.ctx.translate(x, y);
    this.ctx.rotate(rotation);

    switch (entity.type) {
      case 'player':
        this.drawPlayer(physics.size);
        break;
      case 'bullet':
        this.drawBullet(physics.size);
        break;
      case 'pickup':
        this.drawPickup(physics.size);
        break;
      default:
        this.drawGenericEntity(physics.size);
    }

    this.ctx.restore();
  }

  private drawPlayer(size: number): void {
    // Draw triangle ship
    this.ctx.fillStyle = '#00ff00';
    this.ctx.strokeStyle = '#00aa00';
    this.ctx.lineWidth = 2;

    this.ctx.beginPath();
    this.ctx.moveTo(size / 2, 0);
    this.ctx.lineTo(-size / 2, -size / 3);
    this.ctx.lineTo(-size / 2, size / 3);
    this.ctx.closePath();

    this.ctx.fill();
    this.ctx.stroke();
  }

  private drawBullet(size: number): void {
    this.ctx.fillStyle = '#ffff00';
    this.ctx.beginPath();
    this.ctx.arc(0, 0, size / 2, 0, Math.PI * 2);
    this.ctx.fill();
  }

  private drawPickup(size: number): void {
    this.ctx.fillStyle = '#00ffff';
    this.ctx.strokeStyle = '#00aaaa';
    this.ctx.lineWidth = 1;

    // Draw hexagon
    this.ctx.beginPath();
    for (let i = 0; i < 6; i++) {
      const angle = (Math.PI * 2 / 6) * i;
      const x = Math.cos(angle) * size / 2;
      const y = Math.sin(angle) * size / 2;
      if (i === 0) {
        this.ctx.moveTo(x, y);
      } else {
        this.ctx.lineTo(x, y);
      }
    }
    this.ctx.closePath();

    this.ctx.fill();
    this.ctx.stroke();
  }

  private drawGenericEntity(size: number): void {
    this.ctx.fillStyle = '#888888';
    this.ctx.fillRect(-size / 2, -size / 2, size, size);
  }

  private renderEntityUI(entity: Entity): void {
    const physics = entity.getComponent<PhysicsComponent>('physics');
    if (!physics) return;

    const network = entity.getComponent<NetworkComponent>('network');

    // Calculate interpolated position
    let x = physics.position.x;
    let y = physics.position.y;

    if (network && !network.isLocallyControlled) {
      x = this.lerp(physics.lastPosition.x, physics.position.x, this.interpolationAlpha);
      y = this.lerp(physics.lastPosition.y, physics.position.y, this.interpolationAlpha);
    }

    // Draw health bar
    const health = entity.getComponent<HealthComponent>('health');
    if (health && health.current < health.max) {
      this.drawHealthBar(x, y - physics.size - 10, health.current / health.max);
    }

    // Draw energy bar
    const energy = entity.getComponent<EnergyComponent>('energy');
    if (energy && energy.current < energy.max) {
      this.drawEnergyBar(x, y - physics.size - 20, energy.current / energy.max);
    }

    // Draw name tag
    const nameTag = entity.getComponent<{ name: string }>('nameTag');
    if (nameTag) {
      this.drawNameTag(x, y + physics.size + 15, nameTag.name);
    }
  }

  private drawHealthBar(x: number, y: number, percentage: number): void {
    const width = 40;
    const height = 4;

    // Background
    this.ctx.fillStyle = 'rgba(255, 255, 255, 0.2)';
    this.ctx.fillRect(x - width / 2, y - height / 2, width, height);

    // Foreground
    this.ctx.fillStyle = percentage > 0.3 ? '#00ff00' : '#ff0000';
    this.ctx.fillRect(x - width / 2, y - height / 2, width * percentage, height);
  }

  private drawEnergyBar(x: number, y: number, percentage: number): void {
    const width = 40;
    const height = 4;

    // Background
    this.ctx.fillStyle = 'rgba(255, 255, 255, 0.2)';
    this.ctx.fillRect(x - width / 2, y - height / 2, width, height);

    // Foreground
    this.ctx.fillStyle = '#00ffff';
    this.ctx.fillRect(x - width / 2, y - height / 2, width * percentage, height);
  }

  private drawNameTag(x: number, y: number, name: string): void {
    this.ctx.save();
    this.ctx.fillStyle = '#ffffff';
    this.ctx.font = '12px Arial';
    this.ctx.textAlign = 'center';
    this.ctx.textBaseline = 'middle';
    this.ctx.fillText(name, x, y);
    this.ctx.restore();
  }

  onEntityChanged(entity: Entity): void {
    // Reset interpolation when entity changes significantly
    const network = entity.getComponent<NetworkComponent>('network');
    if (network && !network.isLocallyControlled) {
      this.interpolationAlpha = 0;
    }
  }
}