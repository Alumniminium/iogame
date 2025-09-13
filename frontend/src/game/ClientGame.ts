import { World } from '../ecs/core/World';
import { NetworkSystem } from '../ecs/systems/NetworkSystem';
import { PhysicsSystem } from '../ecs/systems/PhysicsSystem';
import { InputSystem } from '../ecs/systems/InputSystem';
import { ClientRenderSystem } from '../ecs/systems/ClientRenderSystem';
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
    
    // Initialize core systems
    this.world = new World();
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
    
    // Add systems to world in proper order
    this.world.addSystem(this.inputSystem);      // Process input first
    this.world.addSystem(this.physicsSystem);    // Apply physics
    this.world.addSystem(this.networkSystem);    // Handle networking
    this.world.addSystem(this.renderSystem);     // Render last
    
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
    this.world.update(deltaTime);

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
      const entity = this.world.getEntity(this.localPlayerId);
      if (entity) {
        this.renderSystem.followEntity(entity);
      }
    }
  }
  
  private render(): void {
    // Get all entities for rendering
    const entities = Array.from(this.world['entities'].values());
    
    // Render the scene
    this.renderSystem.render(entities);
    
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
      ctx.fillText(`Entities: ${this.world['entities'].size}`, 10, 80);
      ctx.fillText(`Bytes In: ${this.formatBytes(stats.bytesReceived)}`, 10, 100);
      ctx.fillText(`Bytes Out: ${this.formatBytes(stats.bytesSent)}`, 10, 120);
      ctx.fillText(`Packets In: ${stats.packetsReceived}`, 10, 140);
      ctx.fillText(`Packets Out: ${stats.packetsSent}`, 10, 160);
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
    this.localPlayerId = playerId;
    this.inputSystem.setLocalEntity(playerId);
    this.networkSystem.setLocalEntity(playerId);
  }
  
  sendChat(message: string): void {
    this.networkManager.sendChat(message);
  }
  
  disconnect(): void {
    this.stop();
    this.networkManager.disconnect();
  }
  
  getWorld(): World {
    return this.world;
  }
  
  getNetworkManager(): NetworkManager {
    return this.networkManager;
  }

  private getLocalPlayerStats(): EntityStats | undefined {
    const gameEntities = (window as any).gameEntities as Map<number, any>;

    if (this.localPlayerId && gameEntities && gameEntities.has(this.localPlayerId)) {
      const entity = gameEntities.get(this.localPlayerId);
      const inputState = this.inputManager.getInputState();

      // Use actual throttle from server if available, otherwise calculate from input
      let throttlePercentage = entity.throttle ? Math.round(entity.throttle * 100) : 0;

      if (throttlePercentage === 0) {
        // Fallback to input-based calculation if no server data yet
        if (inputState.thrust) {
          throttlePercentage = 100;
        } else if (inputState.invThrust) {
          throttlePercentage = 50; // Show as positive value
        }
      }

      return {
        health: entity.health !== undefined && entity.maxHealth !== undefined ? {
          current: entity.health,
          max: entity.maxHealth
        } : undefined,
        energy: entity.energy !== undefined && entity.maxEnergy !== undefined ? {
          current: entity.energy,
          max: entity.maxEnergy
        } : undefined,
        engine: {
          throttle: throttlePercentage,
          powerDraw: entity.enginePowerDraw || (throttlePercentage > 0 ? 50.0 : 0),
          rcsActive: inputState.rcs
        }
      };
    }

    return undefined;
  }

  private drawStatusBars(): void {
    // Get player data
    const playerStats = this.getLocalPlayerStats();

    // Log player status bar data (only once per second to avoid spam)
    const now = Date.now();
    if (!this.lastStatusLogTime || now - this.lastStatusLogTime > 1000) {
      const gameEntities = (window as any).gameEntities as Map<number, any>;
      const localEntity = this.localPlayerId && gameEntities?.get(this.localPlayerId);

      if (localEntity) {
        console.log(`🎮 Player Entity Data:`, {
          health: `${localEntity.health}/${localEntity.maxHealth}`,
          energy: `${localEntity.energy}/${localEntity.maxEnergy}`,
          shield: `${localEntity.shieldCharge}/${localEntity.shieldMaxCharge}`,
          throttle: `${((localEntity.throttle || 0) * 100).toFixed(1)}%`,
          enginePower: `${localEntity.enginePowerDraw || 0}kW`,
          shieldRechargeRate: `${localEntity.shieldRechargeRate || 0}/s`,
          shieldPowerUse: `${localEntity.shieldPowerUse || 0}kW`
        });

        // Shield-specific debugging
        const inputState = this.inputManager.getInputState();
        if (localEntity.shieldCharge === 0) {
          console.log(`🛡️ SHIELD DEBUG: Shield at 0, checking conditions:`, {
            shieldPressed: inputState.shield,
            energy: localEntity.energy,
            maxEnergy: localEntity.maxEnergy,
            energyPercentage: ((localEntity.energy / localEntity.maxEnergy) * 100).toFixed(1) + '%'
          });
        }
      }
      this.lastStatusLogTime = now;
    }
    if (playerStats) {
      // Get actual entity data for real min/max values
      const gameEntities = (window as any).gameEntities as Map<number, any>;
      const localEntity = this.localPlayerId && gameEntities?.get(this.localPlayerId);

      // Health bar with server values
      const healthBar: BarData | undefined = localEntity?.health !== undefined ? {
        current: localEntity.health,
        max: localEntity.maxHealth || 100,
        label: 'Health'
      } : undefined;

      // Energy bar with server values
      const energyBar: BarData | undefined = localEntity?.energy !== undefined ? {
        current: localEntity.energy,
        max: localEntity.maxEnergy || 1000,
        label: 'Energy'
      } : undefined;

      // Shield bar with server values
      const shieldBar: BarData | undefined = localEntity?.shieldCharge !== undefined ? {
        current: localEntity.shieldCharge,
        max: localEntity.shieldMaxCharge || 100,
        label: 'Shield'
      } : undefined;

      this.statusBarManager.renderPlayerBars(healthBar, energyBar, shieldBar);
    }

    // Get entities for target bars
    const gameEntities = (window as any).gameEntities as Map<number, any>;
    if (gameEntities) {
      const targets: StatusBarData[] = [];

      // Get camera info for positioning
      const localPos = (window as any).localPlayerPosition || { x: 0, y: 0 };
      const viewDistance = (window as any).viewDistance || 300;
      const screenSize = Math.min(this.canvas.width, this.canvas.height);
      const camera = {
        x: localPos.x,
        y: localPos.y,
        zoom: screenSize / (viewDistance * 2)
      };

      gameEntities.forEach((entity, entityId) => {
        // Skip local player
        if (entityId === this.localPlayerId) return;

        // Only show bars for entities within reasonable distance
        const distance = Math.sqrt(
          Math.pow(entity.position.x - localPos.x, 2) +
          Math.pow(entity.position.y - localPos.y, 2)
        );

        if (distance < viewDistance * 0.8) { // Show bars for entities within 80% of view distance
          // Calculate screen position for bars below entity
          const barPosition = this.statusBarManager.getEntityBarPosition(
            entity.position.x,
            entity.position.y,
            entity.size || 20,
            camera
          );

          // Only show bars if entity is visible on screen
          if (barPosition.x > -150 && barPosition.x < this.canvas.width + 50 &&
              barPosition.y > -50 && barPosition.y < this.canvas.height + 50) {

            // Only show bars with actual data
            const statusBarData: StatusBarData = {
              entityId,
              position: barPosition,
              title: entity.name || `Entity ${entityId}`
            };

            // Add health bar if we have health data
            if (entity.health !== undefined && entity.maxHealth !== undefined) {
              statusBarData.health = {
                current: entity.health,
                max: entity.maxHealth,
                label: 'HP'
              };
            }

            // Add energy bar if we have energy data
            if (entity.energy !== undefined && entity.maxEnergy !== undefined) {
              statusBarData.energy = {
                current: entity.energy,
                max: entity.maxEnergy,
                label: 'EN'
              };
            }

            // Add shield bar if we have shield data
            if (entity.shieldCharge !== undefined && entity.shieldMaxCharge !== undefined) {
              statusBarData.shield = {
                current: entity.shieldCharge,
                max: entity.shieldMaxCharge,
                label: 'SH'
              };
            }

            targets.push(statusBarData);
          }
        }
      });

      this.statusBarManager.renderTargetBars(targets);

      // Log target bars info (only once per second to avoid spam)
      if (!this.lastStatusLogTime || now - this.lastStatusLogTime > 1000) {
        if (targets.length > 0) {
          console.log(`🎯 Target Status Bars: ${targets.length} entities with bars visible`);
          targets.forEach(target => {
            const hasHealth = target.health ? 'H' : '-';
            const hasEnergy = target.energy ? 'E' : '-';
            const hasShield = target.shield ? 'S' : '-';
            console.log(`  Entity ${target.entityId} [${hasHealth}${hasEnergy}${hasShield}]: ${target.title}`);
          });
        } else {
          console.log(`🎯 Target Status Bars: No visible targets`);
        }
      }
    }
  }
}