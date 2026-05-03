import { Container, Graphics, Text } from "pixi.js";
import { FrappeTheme } from "../theme/colors";

const defaultButtonOptions = {
  text: "",
  width: 301,
  height: 112,
  fontSize: 28,
  onPress: undefined as (() => void) | undefined,
};

type ButtonOptions = typeof defaultButtonOptions;

export class Button extends Container {
  private callbacks = {
    onPress: [] as (() => void)[],
  };

  public onPress = {
    connect: (fn: () => void) => this.callbacks.onPress.push(fn),
  };

  private background: Graphics;
  private textLabel: Text;
  private isPressed = false;

  constructor(options: Partial<ButtonOptions> = {}) {
    const opts = { ...defaultButtonOptions, ...options };

    super();

    if (opts.onPress) {
      this.onPress.connect(opts.onPress);
    }

    this.background = new Graphics();
    this.background.rect(0, 0, opts.width, opts.height).fill({ color: FrappeTheme.ui.button }).stroke({ width: 2, color: FrappeTheme.ui.border });
    this.addChild(this.background);

    this.textLabel = new Text({
      text: opts.text,
      style: {
        fontFamily: "Arial",
        fontSize: opts.fontSize,
        fill: FrappeTheme.text.primary,
        align: "center",
      },
    });
    this.textLabel.x = opts.width / 2 - this.textLabel.width / 2;
    this.textLabel.y = opts.height / 2 - this.textLabel.height / 2;
    this.addChild(this.textLabel);

    this.eventMode = "static";
    this.cursor = "pointer";

    this.on("pointerdown", () => {
      this.background.tint = FrappeTheme.ui.hover;
    });

    this.on("pointerup", () => {
      this.background.tint = 0xffffff;
      this.callbacks.onPress.forEach((fn) => fn());
    });

    this.on("pointerover", () => {
      this.background.tint = FrappeTheme.ui.buttonActive;
    });

    this.on("pointerout", () => {
      this.background.tint = this.isPressed ? FrappeTheme.ui.success : 0xffffff;
    });
  }

  setPressed(pressed: boolean): void {
    this.isPressed = pressed;
    this.background.tint = pressed ? FrappeTheme.ui.success : 0xffffff;
  }

  getPressed(): boolean {
    return this.isPressed;
  }
}
