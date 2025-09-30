import { Container, Graphics, Text } from "pixi.js";
import { Button } from "../Button";

export type ShapeType = "box" | "triangle";

export class ShapeSelector extends Container {
  private background: Graphics;
  private boxButton: Button;
  private triangleButton: Button;
  private onShapeSelected: ((shape: ShapeType) => void) | null = null;

  constructor() {
    super();

    this.background = new Graphics();
    this.background.rect(0, 0, 300, 60).fill({ color: 0x2a2a2a, alpha: 0.9 });

    const title = new Text({
      text: "Select Shape:",
      style: { fill: 0xffffff, fontSize: 14 },
    });
    title.x = 10;
    title.y = 5;

    this.boxButton = new Button({
      text: "Box",
      width: 120,
      height: 35,
      onPress: () => this.handleShapeSelect("box"),
    });
    this.boxButton.x = 10;
    this.boxButton.y = 20;

    this.triangleButton = new Button({
      text: "Triangle",
      width: 120,
      height: 35,
      onPress: () => this.handleShapeSelect("triangle"),
    });
    this.triangleButton.x = 140;
    this.triangleButton.y = 20;

    this.addChild(this.background);
    this.addChild(title);
    this.addChild(this.boxButton);
    this.addChild(this.triangleButton);

    this.visible = false;
  }

  setOnShapeSelected(callback: (shape: ShapeType) => void): void {
    this.onShapeSelected = callback;
  }

  private handleShapeSelect(shape: ShapeType): void {
    if (this.onShapeSelected) {
      this.onShapeSelected(shape);
    }
  }

  show(): void {
    this.visible = true;
  }

  hide(): void {
    this.visible = false;
  }
}
