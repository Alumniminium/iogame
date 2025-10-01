import { Container, Graphics } from "pixi.js";
import { BaseRenderer } from "./BaseRenderer";

/**
 * Renders temporary visual effects like lines and markers
 */
export class EffectRenderer extends BaseRenderer {
  private lineGraphics: Graphics[] = [];
  private renderLineListener: (event: Event) => void;

  constructor(gameContainer: Container) {
    super(gameContainer);
    this.renderLineListener = this.handleRenderLine.bind(this);
    window.addEventListener("render-line", this.renderLineListener);
  }

  update(_deltaTime: number): void {
    // Effects are event-driven, no per-frame updates needed
  }

  private handleRenderLine(event: Event): void {
    const customEvent = event as CustomEvent;
    const { origin, hit, color, duration } = customEvent.detail;

    const lineGraphic = new Graphics();

    lineGraphic
      .moveTo(origin.x, origin.y)
      .lineTo(hit.x, hit.y)
      .stroke({ width: 0.2, color: color || 0xff0000 });

    this.gameContainer.addChild(lineGraphic);
    this.lineGraphics.push(lineGraphic);

    setTimeout(() => {
      if (this.gameContainer.children.includes(lineGraphic)) {
        this.gameContainer.removeChild(lineGraphic);
        lineGraphic.destroy();
      }
      const index = this.lineGraphics.indexOf(lineGraphic);
      if (index > -1) {
        this.lineGraphics.splice(index, 1);
      }
    }, duration || 1000);
  }

  cleanup(): void {
    super.cleanup();

    window.removeEventListener("render-line", this.renderLineListener);

    this.lineGraphics.forEach((graphic) => {
      if (this.gameContainer.children.includes(graphic)) {
        this.gameContainer.removeChild(graphic);
      }
      graphic.destroy();
    });
    this.lineGraphics = [];
  }
}
