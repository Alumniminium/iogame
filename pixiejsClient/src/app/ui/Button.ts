import { Container, Graphics, Text } from "pixi.js";

const defaultButtonOptions = {
  text: "",
  width: 301,
  height: 112,
  fontSize: 28,
};

type ButtonOptions = typeof defaultButtonOptions;

export class Button extends Container {
  private callbacks = {
    onDown: [] as Function[],
    onHover: [] as Function[],
    onPress: [] as Function[],
  };

  public onDown = { connect: (fn: Function) => this.callbacks.onDown.push(fn) };
  public onHover = {
    connect: (fn: Function) => this.callbacks.onHover.push(fn),
  };
  public onPress = {
    connect: (fn: Function) => this.callbacks.onPress.push(fn),
  };

  private background: Graphics;
  private textLabel: Text;

  constructor(options: Partial<ButtonOptions> = {}) {
    const opts = { ...defaultButtonOptions, ...options };

    super();

    this.background = new Graphics();
    this.background
      .rect(0, 0, opts.width, opts.height)
      .fill({ color: 0x333333 })
      .stroke({ width: 2, color: 0x555555 });
    this.addChild(this.background);

    this.textLabel = new Text({
      text: opts.text,
      style: {
        fontFamily: "Arial",
        fontSize: opts.fontSize,
        fill: 0xffffff,
        align: "center",
      },
    });
    this.textLabel.x = opts.width / 2 - this.textLabel.width / 2;
    this.textLabel.y = opts.height / 2 - this.textLabel.height / 2;
    this.addChild(this.textLabel);

    this.eventMode = "static";
    this.cursor = "pointer";

    this.on("pointerdown", () => {
      this.background.tint = 0xcccccc;
      this.callbacks.onDown.forEach((fn) => fn());
    });

    this.on("pointerup", () => {
      this.background.tint = 0xffffff;
      this.callbacks.onPress.forEach((fn) => fn());
    });

    this.on("pointerover", () => {
      this.background.tint = 0xdddddd;
      this.callbacks.onHover.forEach((fn) => fn());
    });

    this.on("pointerout", () => {
      this.background.tint = 0xffffff;
    });
  }
}
