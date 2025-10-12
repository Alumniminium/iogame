import { Container, Graphics } from "pixi.js";
import { System1 } from "../../core/System";
import { NTT } from "../../core/NTT";
import { LineComponent } from "../../components/LineComponent";
import { RenderComponent } from "../../components/RenderComponent";

export class LineRenderer extends System1<LineComponent> {
  private gameContainer: Container;

  constructor(gameContainer: Container) {
    super(LineComponent);
    this.gameContainer = gameContainer;
  }

  protected updateEntity(ntt: NTT, lc: LineComponent, _deltaTime: number): void {
    const render = ntt.get(RenderComponent);
    if (!render) return;

    const now = Date.now();

    let graphics = render.renderers.get(LineComponent);
    if (!graphics) {
      graphics = new Graphics();
      lc.createdAt = now;
      this.gameContainer.addChild(graphics);
      render.renderers.set(LineComponent, graphics);
    }

    const elapsed = now - (lc.createdAt ?? now);
    const alpha = Math.max(0, 1 - elapsed / lc.duration);

    if (alpha <= 0) {
      graphics.visible = false;
      graphics.clear();
      return;
    }

    graphics.visible = true;
    graphics.clear();
    graphics.moveTo(lc.origin.x, lc.origin.y);
    graphics.lineTo(lc.hit.x, lc.hit.y);
    graphics.stroke({ width: 0.15, color: lc.color, alpha });
  }
}
