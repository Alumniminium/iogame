import { HealthBar } from './components/HealthBar';
import { LevelDisplay } from './components/LevelDisplay';
import { EnergyBar } from './components/EnergyBar';

export class UiManager {
  private uiContainer: HTMLElement;
  private uiComponents = new Map<number, HTMLElement[]>();

  constructor(containerId: string = 'ui-container') {
    this.uiContainer = document.getElementById(containerId) || document.body;
  }

  createPlayerUI(entityId: number): void {
    const components = [
      new HealthBar(entityId).getElement(),
      new LevelDisplay(entityId).getElement(),
      new EnergyBar(entityId).getElement()
    ];

    this.uiComponents.set(entityId, components);
    components.forEach(component => this.uiContainer.appendChild(component));
  }

  removePlayerUI(entityId: number): void {
    const components = this.uiComponents.get(entityId);
    if (components) {
      components.forEach(component => {
        if (component.parentNode) {
          component.parentNode.removeChild(component);
        }
      });
      this.uiComponents.delete(entityId);
    }
  }

  updateLayout(): void {
    // Update UI layout based on screen size or game state
    const components = Array.from(this.uiComponents.values()).flat();
    components.forEach(component => {
      // Add responsive layout logic here
    });
  }
}