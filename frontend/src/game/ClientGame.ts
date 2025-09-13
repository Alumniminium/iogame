import { World } from '../ecs/core/World';
import { Entity } from '../ecs/core/Entity';
import { EntityType } from '../ecs/core/types';
import { NetworkSystem } from '../ecs/systems/NetworkSystem';
import { PhysicsSystem } from '../ecs/systems/PhysicsSystem';
import { InputSystem } from '../ecs/systems/InputSystem';
import { ClientRenderSystem } from '../ecs/systems/ClientRenderSystem';
import { PhysicsComponent } from '../ecs/components/PhysicsComponent';
import { NetworkComponent } from '../ecs/components/NetworkComponent';
import { NetworkManager } from '../network/NetworkManager';
import { InputManager } from '../input/InputManager';
import { InputOverlay, EntityStats } from '../ui/components/InputOverlay';
import { StatusBarManager } from '../ui/components/StatusBarManager';
import { BarData, StatusBarData } from '../ui/components/StatusBars';

export interface GameConfig {
  canvas: HTMLCanvasElement;
  playerName: string;
  serverUrl?: string;
}

export class ClientGame {
  private world: World;
  private networkManager: NetworkManager;
  private inputManager: InputManager;
  private renderSystem: ClientRenderSystem;
  private inputSystem: InputSystem;
  private networkSystem: NetworkSystem;
  private physicsSystem: PhysicsSystem;
  private inputOverlay: InputOverlay;
  private statusBarManager: StatusBarManager;

  private lastTime = 0;
  private accumulator = 0;
  private fixedTimeStep = 1 / 60; // 60 Hz fixed update
  private maxAccumulator = 0.2; // Max 200ms worth of updates

  private running = false;
  private localPlayerId: number | null = null;
  private canvas: HTMLCanvasElement;
  
  // Performance metrics
  private fps = 0;
  private frameCount = 0;
  private lastFpsUpdate = 0;
  
  constructor(config: GameConfig) {
    this.canvas = config.canvas;

    // Initialize static World
    World.initialize();
    this.world = World.getInstance();

    this.inputManager = new InputManager(config.canvas);
    this.networkManager = new NetworkManager({
      serverUrl: config.serverUrl,
      interpolationDelay: 100,
      predictionEnabled: true
    });

    // Initialize ECS systems
    this.physicsSystem = new PhysicsSystem();
    this.networkSystem = new NetworkSystem();
    this.inputSystem = new InputSystem(this.inputManager, this.networkManager);
    this.renderSystem = new ClientRenderSystem(config.canvas);

    // Initialize UI overlay (positioned in top-right corner)
    this.inputOverlay = new InputOverlay({
      canvas: config.canvas,
      position: { x: config.canvas.width - 200, y: 10 },
      size: { width: 180, height: 200 }
    });

    // Initialize status bar manager
    this.statusBarManager = new StatusBarManager({
      canvas: config.canvas,
      playerBarsConfig: {
        position: { x: 16, y: 16 },
        barWidth: 250,
        barHeight: 24,
        barSpacing: 8
      },
      targetBarsConfig: {
        barWidth: 120,
        barHeight: 16,
        barSpacing: 4,
        scale: 0.7
      }
    });

    // Add systems to world with proper dependencies and priorities
    World.addSystem('input', this.inputSystem, [], 100);      // Highest priority - process input first
    World.addSystem('physics', this.physicsSystem, ['input'], 90);   // Physics after input
    World.addSystem('network', this.networkSystem, ['input'], 80);   // Network after input
    // Note: render system is handled separately since it doesn't follow the standard ECS update pattern

    // Register callback for when local player ID is set
    this.networkManager.setLocalPlayerCallback((playerId) => {
      this.setLocalPlayer(playerId);
    });

    // Connect to server
    this.connect(config.playerName);
  }
  
  private async connect(playerName: string): Promise<void> {
    console.log(`Connecting as ${playerName}...`);
    
    const connected = await this.networkManager.connect(playerName);
    
    if (connected) {
      console.log('Connected successfully!');
      this.start();
    } else {
      console.error('Failed to connect to server');
    }
  }
  
  start(): void {
    if (this.running) return;
    
    this.running = true;
    this.lastTime = performance.now();
    this.gameLoop(this.lastTime);
  }
  
  stop(): void {
    this.running = false;
  }
  
  private gameLoop = (currentTime: number): void => {
    if (!this.running) return;
    
    // Calculate delta time
    const deltaTime = Math.min((currentTime - this.lastTime) / 1000, this.maxAccumulator);
    this.lastTime = currentTime;
    
    // Update FPS counter
    this.updateFPS(currentTime);
    
    // Accumulate time for fixed timestep
    this.accumulator += deltaTime;
    
    // Fixed timestep updates
    while (this.accumulator >= this.fixedTimeStep) {
      this.fixedUpdate(this.fixedTimeStep);
      this.accumulator -= this.fixedTimeStep;
    }
    
    // Variable timestep update (for smooth rendering)
    this.update(deltaTime);
    
    // Render
    this.render();
    
    // Continue loop
    requestAnimationFrame(this.gameLoop);
  };
  
  private fixedUpdate(deltaTime: number): void {
    // Handle UI toggle inputs
    this.handleUIToggleInputs();

    // Send input to server
    this.sendInput();

    // Update world at fixed timestep
    World.update(deltaTime);

    // Update network manager
    this.networkManager.update(deltaTime);
  }

  private handleUIToggleInputs(): void {
    const input = this.inputManager.getInputState();

    // Toggle input overlay with F12 (use static flag to track since InputManager handles its own toggles)
    if (input.keys.has('F12') && !this.f12WasPressed) {
      this.inputOverlay.toggle();
    }
    this.f12WasPressed = input.keys.has('F12');
  }

  private f12WasPressed = false;
  private lastStatusLogTime = 0;

  private sendInput(): void {
    const input = this.inputManager.getInputState();

    // Get mouse world position
    const camera = { x: 0, y: 0, zoom: 1 };
    const localPos = (window as any).localPlayerPosition;
    if (localPos) {
      camera.x = localPos.x;
      camera.y = localPos.y;
    }

    const mouseWorld = this.inputManager.getMouseWorldPosition(camera);

    // Always send input (server needs regular updates even when idle)
    this.networkManager.sendInput(
      input,
      mouseWorld.x,
      mouseWorld.y
    );

    // Debug logging for input issues (only log when there's actual input)
    if (input.moveX !== 0 || input.moveY !== 0) {
      console.log(`🎮 MOVE: (${input.moveX.toFixed(2)}, ${input.moveY.toFixed(2)})`);
    }
    if (input.fire) {
      console.log(`🔫 FIRE: at world(${mouseWorld.x.toFixed(1)}, ${mouseWorld.y.toFixed(1)})`);
    }
  }
  
  private update(deltaTime: number): void {
    // Update render system interpolation
    this.renderSystem.update(deltaTime);

    // Follow local player with camera
    if (this.localPlayerId) {
      const entity = World.getEntity(this.localPlayerId);
      if (entity) {
        this.renderSystem.followEntity(entity);
      }
    }
  }
  
  private render(): void {
    // Render the scene using the world
    this.renderSystem.render();

    // Draw UI overlay
    this.drawUI();
  }
  
  private drawUI(): void {
    const ctx = this.canvas.getContext('2d')!;

    // Draw FPS counter and basic stats
    ctx.save();
    ctx.fillStyle = '#00ff00';
    ctx.font = '14px monospace';
    ctx.fillText(`FPS: ${this.fps}`, 10, 20);

    // Draw network stats
    const stats = this.networkManager.getStats();
    ctx.fillText(`Ping: ${stats.latency}ms`, 10, 40);
    ctx.fillText(`Connected: ${stats.connected}`, 10, 60);

    // Draw debug info if enabled
    if (this.isDebugMode()) {
      ctx.fillText(`Entities: ${World.getEntityCount()}`, 10, 80);
      ctx.fillText(`Systems: ${World.getSystemCount()}`, 10, 100);
      ctx.fillText(`Bytes In: ${this.formatBytes(stats.bytesReceived)}`, 10, 120);
      ctx.fillText(`Bytes Out: ${this.formatBytes(stats.bytesSent)}`, 10, 140);
      ctx.fillText(`Packets In: ${stats.packetsReceived}`, 10, 160);
      ctx.fillText(`Packets Out: ${stats.packetsSent}`, 10, 180);
    }

    ctx.restore();

    // Draw status bars
    this.drawStatusBars();

    // Draw input overlay with entity stats
    const inputState = this.inputManager.getInputState();
    (window as any).currentInputState = inputState; // Make available to overlay
    const entityStats = this.getLocalPlayerStats();
    this.inputOverlay.render(inputState, entityStats);

    // Draw help text
    ctx.save();
    ctx.fillStyle = '#888888';
    ctx.font = '12px monospace';
    ctx.fillText('F12 - Toggle Input Display', 10, this.canvas.height - 40);
    ctx.fillText('F3 - Toggle Debug Info', 10, this.canvas.height - 25);
    ctx.restore();
  }
  
  private updateFPS(currentTime: number): void {
    this.frameCount++;
    
    if (currentTime - this.lastFpsUpdate >= 1000) {
      this.fps = this.frameCount;
      this.frameCount = 0;
      this.lastFpsUpdate = currentTime;
    }
  }
  
  private formatBytes(bytes: number): string {
    if (bytes < 1024) return `${bytes}B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)}KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)}MB`;
  }
  
  private isDebugMode(): boolean {
    return this.inputManager.isKeyPressed('F3');
  }
  
  setLocalPlayer(playerId: number): void {
    console.log(`🎮 Setting local player ID: ${playerId}`);
    this.localPlayerId = playerId;
    this.inputSystem.setLocalEntity(playerId);
    this.networkSystem.setLocalEntity(playerId);

    // Create ECS entity if it doesn't exist
    let entity = World.getEntity(playerId);
    if (!entity) {
      console.log(`🎮 Creating ECS entity for player ${playerId}`);
      entity = this.createPlayerEntity(playerId);
    } else {
      console.log(`🎮 ECS entity already exists for player ${playerId}`);
    }

    // Follow the local player with the camera
    if (entity) {
      this.renderSystem.followEntity(entity);
      console.log(`📷 Camera now following entity ${entity.id}`);
    }
  }

  private createPlayerEntity(playerId: number): Entity {
    // Check if entity already exists (it should have been created by NetworkManager)
    let entity = World.getEntity(playerId);

    if (entity) {
      console.log(`🎮 Entity ${playerId} already exists, updating local control status`);

      // Update the network component to mark it as locally controlled
      const network = entity.getComponent(NetworkComponent);
      if (network) {
        network.isLocallyControlled = true;
      }

      return entity;
    }

    // Fallback: create entity if it doesn't exist (shouldn't happen normally)
    console.warn(`🎮 Entity ${playerId} doesn't exist, creating fallback entity`);
    entity = World.createEntity(EntityType.Player, playerId);

    // Get position from global state (temporary fallback)
    const globalPos = (window as any).localPlayerPosition || { x: 0, y: 0 };

    // Add physics component
    const physics = new PhysicsComponent(entity.id, {
      position: globalPos,
      size: 32
    });
    entity.addComponent(physics);

    // Add network component
    const network = new NetworkComponent(entity.id, {
      serverId: playerId,
      isLocallyControlled: true
    });
    entity.addComponent(network);

    console.log(`🎮 Created fallback ECS entity ${entity.id} for player ${playerId} at (${globalPos.x}, ${globalPos.y})`);
    return entity;
  }
  
  sendChat(message: string): void {
    this.networkManager.sendChat(message);
  }
  
  disconnect(): void {
    this.stop();
    this.networkManager.disconnect();
    this.networkSystem.disconnect();
    World.destroy();
  }
  
  getWorld(): World {
    return World.getInstance();
  }
  
  getNetworkManager(): NetworkManager {
    return this.networkManager;
  }

  private getLocalPlayerStats(): EntityStats | undefined {
    if (!this.localPlayerId) return undefined;

    const entity = World.getEntity(this.localPlayerId);
    if (!entity) return undefined;

    const inputState = this.inputManager.getInputState();

    // Calculate throttle from input state
    let throttlePercentage = 0;
    if (inputState.thrust) {
      throttlePercentage = 100;
    } else if (inputState.invThrust) {
      throttlePercentage = 50; // Show as positive value
    }

    // TODO: Add actual health/energy components when they're implemented
    // For now, return basic engine stats
    return {
      health: undefined, // Will be populated when HealthComponent exists
      energy: undefined, // Will be populated when EnergyComponent exists
      engine: {
        throttle: throttlePercentage,
        powerDraw: throttlePercentage > 0 ? 50.0 : 0,
        rcsActive: inputState.rcs
      }
    };
  }

  private drawStatusBars(): void {
    // Get player data
    const playerStats = this.getLocalPlayerStats();

    // Log player status bar data (only once per second to avoid spam)
    const now = Date.now();
    if (!this.lastStatusLogTime || now - this.lastStatusLogTime > 1000) {
      if (this.localPlayerId) {
        const entity = World.getEntity(this.localPlayerId);
        if (entity) {
          // TODO: Add proper component-based logging when health/energy components exist
          console.log(`🎮 Local Player Entity:`, {
            id: entity.id,
            type: entity.type,
            components: entity.getComponentCount()
          });
        }
      }
      this.lastStatusLogTime = now;
    }

    if (playerStats) {
      // TODO: Get health/energy from actual components when they're implemented
      // For now, just render engine stats
      this.statusBarManager.renderPlayerBars(undefined, undefined, undefined);
    }

    // Get entities for target bars using ECS
    const entities = World.queryEntitiesWithComponents(PhysicsComponent);
    const targets: StatusBarData[] = [];

    // Get camera info for positioning
    const camera = this.renderSystem.getCamera();
    const viewDistance = 300;

    entities.forEach(entity => {
      // Skip local player
      if (entity.id === this.localPlayerId) return;

      const physics = entity.getComponent(PhysicsComponent)!;

      // Calculate distance to local player
      if (this.localPlayerId) {
        const localEntity = World.getEntity(this.localPlayerId);
        if (localEntity) {
          const localPhysics = localEntity.getComponent(PhysicsComponent);
          if (localPhysics) {
            const distance = Math.sqrt(
              Math.pow(physics.position.x - localPhysics.position.x, 2) +
              Math.pow(physics.position.y - localPhysics.position.y, 2)
            );

            if (distance < viewDistance * 0.8) {
              // Calculate screen position for bars below entity
              const barPosition = this.statusBarManager.getEntityBarPosition(
                physics.position.x,
                physics.position.y,
                physics.size,
                camera
              );

              // Only show bars if entity is visible on screen
              if (barPosition.x > -150 && barPosition.x < this.canvas.width + 50 &&
                  barPosition.y > -50 && barPosition.y < this.canvas.height + 50) {

                const statusBarData: StatusBarData = {
                  entityId: entity.id,
                  position: barPosition,
                  title: `Entity ${entity.id}`
                };

                // TODO: Add health/energy/shield bars when components are implemented

                targets.push(statusBarData);
              }
            }
          }
        }
      }
    });

    this.statusBarManager.renderTargetBars(targets);

    // Log target bars info (only once per second to avoid spam)
    if (!this.lastStatusLogTime || now - this.lastStatusLogTime > 1000) {
      if (targets.length > 0) {
        console.log(`🎯 Target Status Bars: ${targets.length} entities with bars visible`);
      } else {
        console.log(`🎯 Target Status Bars: No visible targets`);
      }
    }
  }
}