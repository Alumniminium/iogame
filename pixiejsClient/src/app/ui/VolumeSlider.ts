import { Container } from "pixi.js";
import { Graphics } from "pixi.js";

import { Label } from "./Label";

/**
 * A volume slider component to be used in the Settings popup.
 */
export class VolumeSlider extends Container {
  public value: number = 100;
  public onUpdate = { connect: (fn: (value: number) => void) => {} };
  /** Message displayed for the slider */
  public messageLabel: Label;

  constructor(label: string, min = -0.1, max = 100, value = 100) {
    const width = 280;
    const height = 20;
    const radius = 20;
    const border = 4;
    const borderColor = 0xec1561;
    const backgroundColor = 0xffffff;

    const bg = new Graphics()
      .roundRect(0, 0, width, height, radius)
      .fill({ color: borderColor })
      .roundRect(
        border,
        border,
        width - border * 2,
        height - border * 2,
        radius,
      )
      .fill({ color: backgroundColor });

    super();

    this.addChild(bg);

    this.value = value;

    this.messageLabel = new Label({
      text: label,
      style: {
        align: "left",
        fill: 0x4a4a4a,
        fontSize: 18,
      },
    });
    this.messageLabel.anchor.x = 0;
    this.messageLabel.x = 10;
    this.messageLabel.y = -18;
    this.addChild(this.messageLabel);
  }
}
