import { Container, Graphics, Text } from "pixi.js";
import { Button } from "../Button";

export interface PauseMenuCallbacks {
  onContinue?: () => void;
  onSettings?: () => void;
  onHelp?: () => void;
  onQuit?: () => void;
}

export class PauseMenu extends Container {
  private overlay!: Graphics;
  private menuContainer!: Container;
  private callbacks: PauseMenuCallbacks;

  constructor(callbacks: PauseMenuCallbacks = {}) {
    super();
    this.callbacks = callbacks;
    this.createUI();
    this.visible = false;
  }

  private createUI(): void {
    // Create semi-transparent overlay to dim the background
    this.overlay = new Graphics();
    this.overlay.rect(0, 0, window.innerWidth, window.innerHeight);
    this.overlay.fill({ color: 0x000000, alpha: 0.7 });
    this.addChild(this.overlay);

    // Create menu container
    this.menuContainer = new Container();
    this.addChild(this.menuContainer);

    // Center the menu on screen
    const centerX = window.innerWidth / 2;
    const centerY = window.innerHeight / 2;

    // Create menu background
    const menuBackground = new Graphics();
    const menuWidth = 300;
    const menuHeight = 400;
    menuBackground.rect(-menuWidth / 2, -menuHeight / 2, menuWidth, menuHeight);
    menuBackground.fill({ color: 0x2a2a2a, alpha: 0.95 });
    menuBackground.stroke({ width: 2, color: 0x555555 });
    this.menuContainer.addChild(menuBackground);

    // Position menu container at center
    this.menuContainer.x = centerX;
    this.menuContainer.y = centerY;

    // Create title
    const title = new Text({
      text: "PAUSED",
      style: {
        fontFamily: "Arial",
        fontSize: 32,
        fill: 0xffffff,
        fontWeight: "bold",
        align: "center",
      },
    });
    title.x = -title.width / 2;
    title.y = -menuHeight / 2 + 30;
    this.menuContainer.addChild(title);

    // Create menu buttons
    const buttonWidth = 200;
    const buttonHeight = 50;
    const buttonSpacing = 70;
    const startY = -80;

    const continueButton = new Button({
      text: "CONTINUE",
      width: buttonWidth,
      height: buttonHeight,
    });
    continueButton.x = -buttonWidth / 2;
    continueButton.y = startY;
    continueButton.onPress.connect(() => {
      this.callbacks.onContinue?.();
    });
    this.menuContainer.addChild(continueButton);

    const settingsButton = new Button({
      text: "SETTINGS",
      width: buttonWidth,
      height: buttonHeight,
    });
    settingsButton.x = -buttonWidth / 2;
    settingsButton.y = startY + buttonSpacing;
    settingsButton.onPress.connect(() => {
      this.callbacks.onSettings?.();
    });
    this.menuContainer.addChild(settingsButton);

    const helpButton = new Button({
      text: "HELP",
      width: buttonWidth,
      height: buttonHeight,
    });
    helpButton.x = -buttonWidth / 2;
    helpButton.y = startY + buttonSpacing * 2;
    helpButton.onPress.connect(() => {
      this.callbacks.onHelp?.();
    });
    this.menuContainer.addChild(helpButton);

    const quitButton = new Button({
      text: "QUIT",
      width: buttonWidth,
      height: buttonHeight,
    });
    quitButton.x = -buttonWidth / 2;
    quitButton.y = startY + buttonSpacing * 3;
    quitButton.onPress.connect(() => {
      this.callbacks.onQuit?.();
    });
    this.menuContainer.addChild(quitButton);

    // Add ESC key hint
    const escHint = new Text({
      text: "Press ESC to continue",
      style: {
        fontFamily: "Arial",
        fontSize: 14,
        fill: 0xaaaaaa,
        align: "center",
      },
    });
    escHint.x = -escHint.width / 2;
    escHint.y = menuHeight / 2 - 30;
    this.menuContainer.addChild(escHint);
  }

  public show(): void {
    this.visible = true;
    this.updateSize();
  }

  public hide(): void {
    this.visible = false;
  }

  public isVisible(): boolean {
    return this.visible;
  }

  private updateSize(): void {
    // Update overlay size to match current window size
    this.overlay.clear();
    this.overlay.rect(0, 0, window.innerWidth, window.innerHeight);
    this.overlay.fill({ color: 0x000000, alpha: 0.7 });

    // Re-center menu container
    this.menuContainer.x = window.innerWidth / 2;
    this.menuContainer.y = window.innerHeight / 2;
  }

  public resize(width: number, height: number): void {
    this.updateSize();
  }
}
