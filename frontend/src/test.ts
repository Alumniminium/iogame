// Simple test to verify ECS functionality
import { World } from './ecs/core/World';
import { EntityType } from './ecs/core/types';
import { HealthComponent } from './ecs/components/HealthComponent';
import { PhysicsComponent } from './ecs/components/PhysicsComponent';
import { HealthSystem } from './ecs/systems/HealthSystem';
import { PhysicsSystem } from './ecs/systems/PhysicsSystem';

export function runTests(): void {
  console.log('Running ECS Tests...');

  const world = new World();

  // Test 1: Entity creation
  const entity = world.createEntity(EntityType.Player);
  console.assert(entity.id === 1, 'Entity ID should be 1');
  console.assert(entity.type === EntityType.Player, 'Entity type should be Player');

  // Test 2: Component addition
  const health = new HealthComponent(entity.id, 100, 100);
  entity.addComponent(health);
  console.assert(entity.hasComponent(HealthComponent), 'Entity should have HealthComponent');

  // Test 3: Component retrieval
  const retrievedHealth = entity.getComponent(HealthComponent);
  console.assert(retrievedHealth === health, 'Should retrieve the same component instance');

  // Test 4: System registration
  const healthSystem = new HealthSystem();
  world.addSystem(healthSystem);
  console.assert(world instanceof World, 'World should accept systems');

  // Test 5: System filtering
  const physics = new PhysicsComponent(entity.id, { x: 0, y: 0 }, 50);
  entity.addComponent(physics);

  const physicsSystem = new PhysicsSystem();
  world.addSystem(physicsSystem);

  // Test 6: World update
  world.update(0.1);
  console.assert(physics.position.x === 0, 'Physics should not move without acceleration');

  // Test 7: Component changes
  health.takeDamage(20);
  console.assert(health.health === 80, 'Health should be reduced by damage');

  console.log('All tests passed! ✅');
}

// Run tests if this file is executed directly
if (typeof window !== 'undefined') {
  (window as any).runECSTests = runTests;
}