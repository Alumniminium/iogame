import { Container, Graphics, Text } from "pixi.js";
import { Button } from "../Button";

export interface PartDefinition {
  type: "hull" | "shield" | "engine";
  shape: "triangle" | "square";
  name: string;
  description: string;
  color: number;
}

export interface PartPaletteConfig {
  width?: number;
  height?: number;
  backgroundColor?: number;
  backgroundAlpha?: number;
}

export class PartPalette extends Container {
  private background: Graphics;
  private partButtons: Button[] = [];
  private titleText: Text;
  private onPartSelected: ((part: PartDefinition) => void) | null = null;

  private availableParts: PartDefinition[] = [
    {
      type: "hull",
      shape: "square",
      name: "Square Hull",
      description: "Basic structural block",
      color: 0x808080,
    },
    {
      type: "hull",
      shape: "triangle",
      name: "Triangle Hull",
      description: "Angled structural block",
      color: 0x808080,
    },
    {
      type: "shield",
      shape: "square",
      name: "Shield Generator",
      description: "Provides shield protection",
      color: 0x0080ff,
    },
    {
      type: "engine",
      shape: "square",
      name: "Engine Block",
      description: "Provides thrust",
      color: 0xff8000,
    },
  ];

  constructor(config: PartPaletteConfig = {}) {
    super();

    const width = config.width || 200;
    const height = config.height || 400;
    const backgroundColor = config.backgroundColor || 0x2a2a2a;
    const backgroundAlpha = config.backgroundAlpha || 0.9;

    // Create background
    this.background = new Graphics();
    this.background
      .rect(0, 0, width, height)
      .fill({ color: backgroundColor, alpha: backgroundAlpha })
      .stroke({ width: 2, color: 0x444444 });
    this.addChild(this.background);

    // Create title
    this.titleText = new Text({
      text: "Ship Parts",
      style: {
        fontFamily: "Arial",
        fontSize: 18,
        fill: 0xffffff,
        align: "center",
      },
    });
    this.titleText.x = width / 2 - this.titleText.width / 2;
    this.titleText.y = 10;
    this.addChild(this.titleText);

    this.createPartButtons(width);
  }

  private createPartButtons(paletteWidth: number): void {
    const buttonWidth = paletteWidth - 20;
    const buttonHeight = 60;
    const startY = 50;
    const spacing = 10;

    this.availableParts.forEach((part, index) => {
      const button = this.createPartButton(part, buttonWidth, buttonHeight);
      button.x = 10;
      button.y = startY + (buttonHeight + spacing) * index;
      this.addChild(button);
      this.partButtons.push(button);
    });
  }

  private createPartButton(part: PartDefinition, width: number, height: number): Button {
    const button = new Button({
      text: part.name,
      width,
      height,
    });

    // Create a custom button appearance with the part's color and shape
    const buttonContainer = new Container();

    // Background
    const bg = new Graphics();
    bg.rect(0, 0, width, height)
      .fill({ color: 0x333333 })
      .stroke({ width: 1, color: 0x555555 });
    buttonContainer.addChild(bg);

    // Shape preview
    const shapePreview = new Graphics();
    const shapeSize = 20;
    const shapeX = 15;
    const shapeY = height / 2;

    if (part.shape === "triangle") {
      shapePreview
        .poly([
          shapeX, shapeY - shapeSize/2,
          shapeX - shapeSize/2, shapeY + shapeSize/2,
          shapeX + shapeSize/2, shapeY + shapeSize/2
        ])
        .fill(part.color);
    } else {
      shapePreview
        .rect(
          shapeX - shapeSize/2,
          shapeY - shapeSize/2,
          shapeSize,
          shapeSize
        )
        .fill(part.color);
    }
    buttonContainer.addChild(shapePreview);

    // Text
    const nameText = new Text({
      text: part.name,
      style: {
        fontFamily: "Arial",
        fontSize: 14,
        fill: 0xffffff,
      },
    });
    nameText.x = 45;
    nameText.y = height / 2 - 15;
    buttonContainer.addChild(nameText);

    const descText = new Text({
      text: part.description,
      style: {
        fontFamily: "Arial",
        fontSize: 10,
        fill: 0xaaaaaa,
      },
    });
    descText.x = 45;
    descText.y = height / 2 + 5;
    buttonContainer.addChild(descText);

    // Add the container to the button
    button.addChild(buttonContainer);

    // Set up interaction
    button.onPress.connect(() => {
      this.selectPart(part);
    });

    // Add hover effects
    button.eventMode = 'static';
    button.on('pointerover', () => {
      bg.tint = 0xdddddd;
    });
    button.on('pointerout', () => {
      bg.tint = 0xffffff;
    });

    return button;
  }

  private selectPart(part: PartDefinition): void {
    console.log(`Selected part: ${part.name}`);

    // Visual feedback for selection
    this.partButtons.forEach((button, index) => {
      const bg = button.children[0]?.children[0] as Graphics;
      if (bg) {
        if (this.availableParts[index] === part) {
          bg.tint = 0x88ff88; // Green tint for selected
        } else {
          bg.tint = 0xffffff; // Normal tint
        }
      }
    });

    if (this.onPartSelected) {
      this.onPartSelected(part);
    }
  }

  setPartSelectedCallback(callback: (part: PartDefinition) => void): void {
    this.onPartSelected = callback;
  }

  show(): void {
    this.visible = true;
    this.alpha = 0;

    // Simple fade in animation
    const animate = () => {
      this.alpha += 0.1;
      if (this.alpha < 1) {
        requestAnimationFrame(animate);
      } else {
        this.alpha = 1;
      }
    };
    animate();
  }

  hide(): void {
    this.visible = false;
  }

  clearSelection(): void {
    this.partButtons.forEach(button => {
      const bg = button.children[0]?.children[0] as Graphics;
      if (bg) {
        bg.tint = 0xffffff;
      }
    });
  }
}