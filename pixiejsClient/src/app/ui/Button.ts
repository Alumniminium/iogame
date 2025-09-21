import { Container } from "pixi.js";

import { engine } from "../getEngine";


const defaultButtonOptions = {
  text: "",
  width: 301,
  height: 112,
  fontSize: 28,
};

type ButtonOptions = typeof defaultButtonOptions;

/**
 * The big rectangle button, with a label, idle and pressed states
 */
export class Button extends Container {
  public onDown = { connect: (_fn: Function) => {} };
  public onHover = { connect: (_fn: Function) => {} };
  public onPress = { connect: (_fn: Function) => {} };

  constructor(options: Partial<ButtonOptions> = {}) {
    const opts = { ...defaultButtonOptions, ...options };

    super();

    // Create a simple button implementation
    // TODO: Implement proper button graphics and functionality

    this.width = opts.width;
    this.height = opts.height;

    this.onDown.connect(this.handleDown.bind(this));
    this.onHover.connect(this.handleHover.bind(this));
  }

  private handleHover() {
    engine().audio.sfx.play("main/sounds/sfx-hover.wav");
  }

  private handleDown() {
    engine().audio.sfx.play("main/sounds/sfx-press.wav");
  }
}
