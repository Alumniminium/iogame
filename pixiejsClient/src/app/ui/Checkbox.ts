import { Container, Graphics, Text } from "pixi.js";

const defaultCheckboxOptions = {
  label: "",
  checked: false,
  fontSize: 16,
  onChange: undefined as ((checked: boolean) => void) | undefined,
};

type CheckboxOptions = typeof defaultCheckboxOptions;

export class Checkbox extends Container {
  private callbacks = {
    onChange: [] as ((checked: boolean) => void)[],
  };

  public onChange = {
    connect: (fn: (checked: boolean) => void) => this.callbacks.onChange.push(fn),
  };

  private background: Graphics;
  private checkmark: Graphics;
  private textLabel: Text;
  private _checked = false;
  private boxSize = 20;

  constructor(options: Partial<CheckboxOptions> = {}) {
    const opts = { ...defaultCheckboxOptions, ...options };

    super();

    this._checked = opts.checked;

    if (opts.onChange) {
      this.onChange.connect(opts.onChange);
    }

    // Create checkbox box
    this.background = new Graphics();
    this.background.rect(0, 0, this.boxSize, this.boxSize).fill({ color: 0x333333 }).stroke({ width: 2, color: 0x555555 });
    this.addChild(this.background);

    // Create checkmark
    this.checkmark = new Graphics();
    this.checkmark.moveTo(4, 10);
    this.checkmark.lineTo(8, 14);
    this.checkmark.lineTo(16, 6);
    this.checkmark.stroke({ width: 2, color: 0xffffff });
    this.checkmark.visible = this._checked;
    this.addChild(this.checkmark);

    // Create label
    this.textLabel = new Text({
      text: opts.label,
      style: {
        fontFamily: "Arial",
        fontSize: opts.fontSize,
        fill: 0xffffff,
        align: "left",
      },
    });
    this.textLabel.x = this.boxSize + 10;
    this.textLabel.y = this.boxSize / 2 - this.textLabel.height / 2;
    this.addChild(this.textLabel);

    this.eventMode = "static";
    this.cursor = "pointer";

    this.on("pointerdown", () => {
      this.background.tint = 0xcccccc;
    });

    this.on("pointerup", () => {
      this.background.tint = 0xffffff;
      this.toggle();
    });

    this.on("pointerover", () => {
      this.background.tint = 0xdddddd;
    });

    this.on("pointerout", () => {
      this.background.tint = 0xffffff;
    });
  }

  toggle(): void {
    this.setChecked(!this._checked);
  }

  setChecked(checked: boolean): void {
    this._checked = checked;
    this.checkmark.visible = checked;
    this.callbacks.onChange.forEach((fn) => fn(checked));
  }

  getChecked(): boolean {
    return this._checked;
  }
}
