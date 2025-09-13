import { System } from '../core/System';
import { Entity } from '../core/Entity';
import { HealthComponent } from '../components/HealthComponent';
import { LevelComponent } from '../components/LevelComponent';
import { EnergyComponent } from '../components/EnergyComponent';

export class UiSystem extends System {
  readonly componentTypes = [HealthComponent, LevelComponent, EnergyComponent];

  private lastUpdate = new Map<number, number>();

  protected updateEntity(entity: Entity, deltaTime: number): void {
    const lastTick = this.lastUpdate.get(entity.id) || 0;

    // Only update UI if components have changed
    const health = entity.getComponent(HealthComponent);
    const level = entity.getComponent(LevelComponent);
    const energy = entity.getComponent(EnergyComponent);

    if (health && health.changedTick > lastTick) {
      this.updateHealthUI(entity, health);
    }

    if (level && level.changedTick > lastTick) {
      this.updateLevelUI(entity, level);
    }

    if (energy && energy.changedTick > lastTick) {
      this.updateEnergyUI(entity, energy);
    }

    this.lastUpdate.set(entity.id, Date.now());
  }

  private updateHealthUI(entity: Entity, health: HealthComponent): void {
    const healthBar = document.getElementById(`health-${entity.id}`);
    if (healthBar) {
      const percentage = health.healthPercentage;
      (healthBar as HTMLElement).style.width = `${percentage}%`;
    }

    const healthText = document.getElementById(`health-text-${entity.id}`);
    if (healthText) {
      healthText.textContent = `${Math.round(health.health)}/${health.maxHealth}`;
    }
  }

  private updateLevelUI(entity: Entity, level: LevelComponent): void {
    const levelDisplay = document.getElementById(`level-${entity.id}`);
    if (levelDisplay) {
      levelDisplay.textContent = `Level ${level.level}`;
    }

    const expBar = document.getElementById(`exp-${entity.id}`);
    if (expBar) {
      const percentage = level.experiencePercentage;
      (expBar as HTMLElement).style.width = `${percentage}%`;
    }

    const expText = document.getElementById(`exp-text-${entity.id}`);
    if (expText) {
      expText.textContent = `${level.experience}/${level.experienceToNextLevel}`;
    }
  }

  private updateEnergyUI(entity: Entity, energy: EnergyComponent): void {
    const energyBar = document.getElementById(`energy-${entity.id}`);
    if (energyBar) {
      const percentage = energy.energyPercentage;
      (energyBar as HTMLElement).style.width = `${percentage}%`;
    }

    const energyText = document.getElementById(`energy-text-${entity.id}`);
    if (energyText) {
      energyText.textContent = `${Math.round(energy.availableCharge)}/${energy.batteryCapacity}`;
    }
  }
}