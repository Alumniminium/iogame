import { Container, Graphics, Text } from "pixi.js";
import { Button } from "../Button";
import { ShipTemplate, ShipTemplates } from "../../shipbuilder/ShipTemplate";

export interface TemplateSelectorConfig {
  width?: number;
  height?: number;
  backgroundColor?: number;
  backgroundAlpha?: number;
}

export class TemplateSelector extends Container {
  private background: Graphics;
  private titleText: Text;
  private templateButtons: Button[] = [];
  private onTemplateSelected: ((template: ShipTemplate) => void) | null = null;

  constructor(config: TemplateSelectorConfig = {}) {
    super();

    const width = config.width || 200;
    const height = config.height || 300;
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
      text: "Ship Templates",
      style: {
        fontFamily: "Arial",
        fontSize: 16,
        fill: 0xffffff,
        align: "center",
      },
    });
    this.titleText.x = width / 2 - this.titleText.width / 2;
    this.titleText.y = 10;
    this.addChild(this.titleText);

    this.createTemplateButtons(width, height);
  }

  private createTemplateButtons(width: number, _height: number): void {
    const templates = ShipTemplates.getAllTemplates();
    const buttonWidth = width - 20;
    const buttonHeight = 50;
    const startY = 40;
    const spacing = 5;

    templates.forEach((template, index) => {
      const button = this.createTemplateButton(
        template,
        buttonWidth,
        buttonHeight,
      );
      button.x = 10;
      button.y = startY + (buttonHeight + spacing) * index;
      this.addChild(button);
      this.templateButtons.push(button);
    });
  }

  private createTemplateButton(
    template: ShipTemplate,
    width: number,
    height: number,
  ): Button {
    const button = new Button({
      text: template.name,
      width,
      height,
    });

    // Create custom button appearance
    const buttonContainer = new Container();

    // Background
    const bg = new Graphics();
    bg.rect(0, 0, width, height)
      .fill({ color: 0x333333 })
      .stroke({ width: 1, color: 0x555555 });
    buttonContainer.addChild(bg);

    // Ship preview (simplified representation)
    const previewContainer = new Container();
    previewContainer.x = 10;
    previewContainer.y = height / 2;
    previewContainer.scale.set(0.3); // Scale down the preview

    // Draw simplified ship preview
    template.parts.forEach((part) => {
      const partGraphic = new Graphics();
      const size = 8;
      const x = part.gridX * 10;
      const y = part.gridY * 10;

      let color = 0x808080; // Default gray for hull
      if (part.type === "shield") color = 0x0080ff; // Blue for shield
      if (part.type === "engine") color = 0xff8000; // Orange for engine

      if (part.shape === "triangle") {
        partGraphic
          .poly([
            x,
            y - size / 2,
            x - size / 2,
            y + size / 2,
            x + size / 2,
            y + size / 2,
          ])
          .fill(color);
      } else {
        partGraphic.rect(x - size / 2, y - size / 2, size, size).fill(color);
      }

      previewContainer.addChild(partGraphic);
    });

    buttonContainer.addChild(previewContainer);

    // Template name
    const nameText = new Text({
      text: template.name,
      style: {
        fontFamily: "Arial",
        fontSize: 12,
        fill: 0xffffff,
        fontWeight: "bold",
      },
    });
    nameText.x = 80;
    nameText.y = 8;
    buttonContainer.addChild(nameText);

    // Template description
    const descText = new Text({
      text: template.description,
      style: {
        fontFamily: "Arial",
        fontSize: 9,
        fill: 0xcccccc,
        wordWrap: true,
        wordWrapWidth: width - 90,
      },
    });
    descText.x = 80;
    descText.y = 25;
    buttonContainer.addChild(descText);

    button.addChild(buttonContainer);

    // Set up interaction
    button.onPress.connect(() => {
      this.selectTemplate(template);
    });

    // Add hover effects
    button.eventMode = "static";
    button.on("pointerover", () => {
      bg.tint = 0xdddddd;
    });
    button.on("pointerout", () => {
      bg.tint = 0xffffff;
    });

    return button;
  }

  private selectTemplate(template: ShipTemplate): void {
    console.log(`Selected template: ${template.name}`);

    if (this.onTemplateSelected) {
      this.onTemplateSelected(template);
    }
  }

  setTemplateSelectedCallback(
    callback: (template: ShipTemplate) => void,
  ): void {
    this.onTemplateSelected = callback;
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
}
