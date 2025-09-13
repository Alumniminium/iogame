import { Component } from '../core/Component';

export class LevelComponent extends Component {
  level: number;
  experience: number;
  experienceToNextLevel: number;

  constructor(entityId: number, level: number = 1, experience: number = 0, experienceToNextLevel: number = 100) {
    super(entityId);
    this.level = level;
    this.experience = experience;
    this.experienceToNextLevel = experienceToNextLevel;
  }

  addExperience(amount: number): void {
    this.experience += amount;

    while (this.experience >= this.experienceToNextLevel) {
      this.levelUp();
    }

    this.markChanged();
  }

  private levelUp(): void {
    this.experience -= this.experienceToNextLevel;
    this.level++;
    this.experienceToNextLevel = Math.floor(this.experienceToNextLevel * 1.2);
  }

  get experiencePercentage(): number {
    return (this.experience / this.experienceToNextLevel) * 100;
  }
}