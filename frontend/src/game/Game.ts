import { World } from '../ecs/core/World';
import { HealthSystem } from '../ecs/systems/HealthSystem';
import { PhysicsSystem } from '../ecs/systems/PhysicsSystem';
import { UiSystem } from '../ecs/systems/UiSystem';
import { RenderSystem } from '../ecs/systems/RenderSystem';
import { Player } from './entities/Player';

export class Game {
  private world: World;
  private renderSystem: RenderSystem;
  private player: Player;
  private lastTime = 0;
  private animationId: number | null = null;

  constructor(canvas: HTMLCanvasElement) {
    this.world = new World();

    // Initialize systems
    this.world.addSystem(new HealthSystem());
    this.world.addSystem(new PhysicsSystem());
    this.world.addSystem(new UiSystem());
    this.renderSystem = new RenderSystem(canvas);
    this.world.addSystem(this.renderSystem);

    // Create player
    this.player = new Player(1, { x: canvas.width / 2, y: canvas.height / 2 });
  }

  start(): void {
    this.gameLoop(0);
  }

  stop(): void {
    if (this.animationId) {
      cancelAnimationFrame(this.animationId);
      this.animationId = null;
    }
  }

  private gameLoop = (currentTime: number): void => {
    const deltaTime = (currentTime - this.lastTime) / 1000;
    this.lastTime = currentTime;

    // Update ECS world
    this.world.update(deltaTime);

    // Clear and render
    this.renderSystem.clear();

    // Continue loop
    this.animationId = requestAnimationFrame(this.gameLoop);
  }

  getPlayer(): Player {
    return this.player;
  }

  // Input handling
  handleInput(keys: Set<string>): void {
    let moveX = 0;
    let moveY = 0;

    if (keys.has('ArrowLeft') || keys.has('KeyA')) moveX -= 1;
    if (keys.has('ArrowRight') || keys.has('KeyD')) moveX += 1;
    if (keys.has('ArrowUp') || keys.has('KeyW')) moveY -= 1;
    if (keys.has('ArrowDown') || keys.has('KeyS')) moveY += 1;

    if (moveX !== 0 || moveY !== 0) {
      this.player.move({ x: moveX, y: moveY });
    } else {
      this.player.stop();
    }
  }
}