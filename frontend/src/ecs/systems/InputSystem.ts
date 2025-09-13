import { System } from '../core/System';
import { Entity } from '../core/Entity';
import { PhysicsComponent } from '../components/PhysicsComponent';
import { NetworkComponent } from '../components/NetworkComponent';
import { InputManager } from '../../input/InputManager';
import { NetworkManager } from '../../network/NetworkManager';

export class InputSystem extends System {
  private inputManager: InputManager;
  private networkManager: NetworkManager;
  private localEntityId: number | null = null;
  private lastInputSentTime = 0;
  private inputSendRate = 1000 / 60; // 60 Hz
  private entities = new Map<number, Entity>();
  
  constructor(inputManager: InputManager, networkManager: NetworkManager) {
    super();
    this.inputManager = inputManager;
    this.networkManager = networkManager;
  }
  
  setLocalEntity(entityId: number): void {
    this.localEntityId = entityId;
  }
  
  update(deltaTime: number): void {
    if (!this.localEntityId) return;
    
    const entity = this.entities.get(this.localEntityId);
    if (!entity) return;
    
    const physics = entity.getComponent<PhysicsComponent>('physics');
    const network = entity.getComponent<NetworkComponent>('network');
    
    if (!physics || !network || !network.isLocallyControlled) return;
    
    // Get current input state
    const input = this.inputManager.getInputState();
    
    // Get mouse world position
    const camera = this.getCamera();
    const mouseWorld = this.inputManager.getMouseWorldPosition(camera);
    
    // Apply input locally for immediate response (client-side prediction)
    this.applyInputToEntity(entity, input, mouseWorld, deltaTime);
    
    // Send input to server at fixed rate
    const currentTime = Date.now();
    if (currentTime - this.lastInputSentTime >= this.inputSendRate) {
      this.networkManager.sendInput(
        input.moveX,
        input.moveY,
        mouseWorld.x,
        mouseWorld.y,
        input.mouseButtons
      );
      this.lastInputSentTime = currentTime;
    }
  }
  
  private applyInputToEntity(
    entity: Entity, 
    input: { moveX: number; moveY: number; mouseButtons: number },
    mouseWorld: { x: number; y: number },
    deltaTime: number
  ): void {
    const physics = entity.getComponent<PhysicsComponent>('physics');
    if (!physics) return;
    
    // Apply movement
    const speed = 200; // pixels per second
    physics.velocity.x = input.moveX * speed;
    physics.velocity.y = input.moveY * speed;
    
    // Calculate rotation to face mouse
    const dx = mouseWorld.x - physics.position.x;
    const dy = mouseWorld.y - physics.position.y;
    physics.rotation = Math.atan2(dy, dx);
    
    // Handle shooting (left mouse button)
    if (input.mouseButtons & 1) {
      this.handleShooting(entity, mouseWorld);
    }
    
    // Handle special abilities
    if (this.inputManager.isKeyPressed('Space')) {
      this.handleBoost(entity);
    }
    
    if (this.inputManager.isKeyPressed('ShiftLeft')) {
      this.handleShield(entity);
    }
  }
  
  private handleShooting(entity: Entity, mouseWorld: { x: number; y: number }): void {
    const weapon = entity.getComponent<any>('weapon');
    if (weapon && weapon.canFire()) {
      // Fire weapon (this would create a bullet entity and send to server)
      weapon.fire(mouseWorld);
    }
  }
  
  private handleBoost(entity: Entity): void {
    const energy = entity.getComponent<any>('energy');
    const physics = entity.getComponent<PhysicsComponent>('physics');
    
    if (energy && physics && energy.current > 10) {
      // Apply boost
      const boostForce = 500;
      physics.velocity.x += Math.cos(physics.rotation) * boostForce;
      physics.velocity.y += Math.sin(physics.rotation) * boostForce;
      
      // Consume energy
      energy.current -= 10;
    }
  }
  
  private handleShield(entity: Entity): void {
    const shield = entity.getComponent<any>('shield');
    const energy = entity.getComponent<any>('energy');
    
    if (shield && energy && energy.current > 0) {
      shield.active = true;
      energy.current -= 0.5; // Drain energy while shield is active
    }
  }
  
  private getCamera(): { x: number; y: number; zoom: number } {
    // Get camera position from local entity
    if (this.localEntityId) {
      const entity = this.entities.get(this.localEntityId);
      if (entity) {
        const physics = entity.getComponent<PhysicsComponent>('physics');
        if (physics) {
          return {
            x: physics.position.x,
            y: physics.position.y,
            zoom: 1
          };
        }
      }
    }
    
    return { x: 0, y: 0, zoom: 1 };
  }
  
  onEntityChanged(entity: Entity): void {
    // Track entities for input processing
    const network = entity.getComponent<NetworkComponent>('network');
    if (network && network.isLocallyControlled) {
      this.entities.set(entity.id, entity);
    } else {
      this.entities.delete(entity.id);
    }
  }
}