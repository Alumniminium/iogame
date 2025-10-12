import { Container, Graphics } from "pixi.js";
import { System1 } from "../../core/System";
import { Entity } from "../../core/Entity";
import { LineComponent } from "../../components/LineComponent";
import { RenderComponent } from "../../components/RenderComponent";

export class LineRenderer extends System1<LineComponent> {
  private gameContainer: Container;

  constructor(gameContainer: Container) {
    super(LineComponent);
    this.gameContainer = gameContainer;
  }

  protected updateEntity(entity: Entity, line: LineComponent, _deltaTime: number): void {
    const render = entity.get(RenderComponent);
    if (!render) return;

    const now = Date.now();

    let graphic = render.renderers.get(LineComponent);
    if (!graphic) {
      graphic = new Graphics();
      line.createdAt = now;
      this.gameContainer.addChild(graphic);
      render.renderers.set(LineComponent, graphic);
    }

    const elapsed = now - (line.createdAt ?? now);
    const alpha = Math.max(0, 1 - elapsed / line.duration);

    if (alpha <= 0) {
      graphic.visible = false;
      graphic.clear();
      return;
    }

    graphic.visible = true;
    graphic.clear();
    graphic.moveTo(line.origin.x, line.origin.y);
    graphic.lineTo(line.hit.x, line.hit.y);
    graphic.stroke({ width: 0.15, color: line.color, alpha });
  }
}
