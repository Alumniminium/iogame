import { Container, Graphics } from "pixi.js";
import { NTT } from "../../core/NTT";

/**
 * Base class for all specialized renderers.
 * Handles common graphics lifecycle and container management.
 */
export abstract class BaseRenderer {
  protected gameContainer: Container;
  protected graphics = new Map<string, Graphics>();

  constructor(gameContainer: Container) {
    this.gameContainer = gameContainer;
  }

  /**
   * Initialize renderer (called once)
   */
  initialize(): void { }

  /**
   * Update renderer (called every frame)
   */
  abstract update(deltaTime: number): void;

  /**
   * Clean up all graphics
   */
  cleanup(): void {
    this.graphics.forEach((graphic) => graphic.destroy());
    this.graphics.clear();
  }

  /**
   * Remove graphics for a specific entity
   */
  removeGraphic(ntt: NTT): void {
    const graphic = this.graphics.get(ntt.id);
    if (graphic) {
      if (this.gameContainer.children.includes(graphic)) {
        this.gameContainer.removeChild(graphic);
      }
      graphic.destroy();
      this.graphics.delete(ntt.id);
    }
  }

  /**
   * Handle entity destruction
   */
  onEntityDestroyed(entity: NTT): void {
    this.removeGraphic(entity);
  }
}
