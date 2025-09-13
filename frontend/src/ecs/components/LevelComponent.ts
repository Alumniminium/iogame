import { Component } from '../core/Component';

export interface LevelConfig {
  level?: number;
  experience?: number;
  experienceToNext?: number;
}

export class LevelComponent extends Component {
  level: number;
  experience: number;
  experienceToNext: number;
  totalExperience: number;

  constructor(entityId: number, config: LevelConfig = {}) {
    super(entityId);

    this.level = config.level || 1;
    this.experience = config.experience || 0;
    // Calculate initial experienceToNext if not provided
    this.experienceToNext = config.experienceToNext || Math.floor(100 * Math.pow(this.level + 1, 1.5));
    this.totalExperience = this.experience;
  }
}