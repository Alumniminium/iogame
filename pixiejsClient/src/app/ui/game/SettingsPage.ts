import { Container, Graphics, Text } from "pixi.js";
import { ScrollBox } from "@pixi/ui";
import { Button } from "../Button";
import { Checkbox } from "../Checkbox";
import {
  KeybindManager,
  type KeybindAction,
} from "../../managers/KeybindManager";
import { userSettings } from "../../utils/userSettings";

export interface SettingsPageCallbacks {
  onClose?: () => void;
}

export class SettingsPage extends Container {
  private overlay!: Graphics;
  private menuContainer!: Container;
  private menuBackground!: Graphics;
  private callbacks: SettingsPageCallbacks;
  private generalTabButton!: Button;
  private keybindsTabButton!: Button;
  private contentContainer!: Container;
  private keybindManager = KeybindManager.getInstance();

  private rebindingAction: KeybindAction | null = null;
  private rebindingKeyIndex: number = 0;
  private keydownHandler: ((e: KeyboardEvent) => void) | null = null;

  private menuWidth = 600;
  private menuHeight = 0;

  constructor(callbacks: SettingsPageCallbacks = {}) {
    super();
    this.callbacks = callbacks;
    this.createUI();
    this.visible = false;
  }

  private createUI(): void {
    // Calculate menu height as 75% of window height
    this.menuHeight = Math.floor(window.innerHeight * 0.75);

    // Create semi-transparent overlay
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
    this.menuBackground = new Graphics();
    this.menuBackground.rect(
      -this.menuWidth / 2,
      -this.menuHeight / 2,
      this.menuWidth,
      this.menuHeight,
    );
    this.menuBackground.fill({ color: 0x2a2a2a, alpha: 0.95 });
    this.menuBackground.stroke({ width: 2, color: 0x555555 });
    this.menuContainer.addChild(this.menuBackground);

    // Position menu container at center
    this.menuContainer.x = centerX;
    this.menuContainer.y = centerY;

    // Create title
    const title = new Text({
      text: "SETTINGS",
      style: {
        fontFamily: "Arial",
        fontSize: 32,
        fill: 0xffffff,
        fontWeight: "bold",
        align: "center",
      },
    });
    title.x = -title.width / 2;
    title.y = -this.menuHeight / 2 + 30;
    this.menuContainer.addChild(title);

    // Create tab buttons
    const tabButtonWidth = 140;
    const tabButtonHeight = 40;
    const tabY = -this.menuHeight / 2 + 90;

    this.generalTabButton = new Button({
      text: "General",
      width: tabButtonWidth,
      height: tabButtonHeight,
      fontSize: 20,
    });
    this.generalTabButton.x = -this.menuWidth / 2 + 20;
    this.generalTabButton.y = tabY;
    this.generalTabButton.onPress.connect(() => this.switchTab("general"));
    this.menuContainer.addChild(this.generalTabButton);

    this.keybindsTabButton = new Button({
      text: "Keybinds",
      width: tabButtonWidth,
      height: tabButtonHeight,
      fontSize: 20,
    });
    this.keybindsTabButton.x = -this.menuWidth / 2 + 20 + tabButtonWidth + 10;
    this.keybindsTabButton.y = tabY;
    this.keybindsTabButton.onPress.connect(() => this.switchTab("keybinds"));
    this.menuContainer.addChild(this.keybindsTabButton);

    // Create content container for tab content
    this.contentContainer = new Container();
    this.contentContainer.y = tabY + tabButtonHeight + 20;
    this.menuContainer.addChild(this.contentContainer);

    // Create X close button in top right
    const closeButton = new Button({
      text: "✕",
      width: 40,
      height: 40,
      fontSize: 24,
    });
    closeButton.x = this.menuWidth / 2 - 50;
    closeButton.y = -this.menuHeight / 2 + 10;
    closeButton.onPress.connect(() => {
      this.stopRebinding();
      this.callbacks.onClose?.();
    });
    this.menuContainer.addChild(closeButton);

    // Initialize with general tab
    this.switchTab("general");
  }

  private switchTab(tab: "general" | "keybinds"): void {
    // Update tab button states
    this.generalTabButton.setPressed(tab === "general");
    this.keybindsTabButton.setPressed(tab === "keybinds");

    // Clear current content
    this.contentContainer.removeChildren();

    // Add content based on current tab
    if (tab === "general") this.createGeneralContent();
    else this.createKeybindsContent();
  }

  private createGeneralContent(): void {
    const content = new Container();

    // Title
    const title = new Text({
      text: "Graphics Settings",
      style: {
        fontFamily: "Arial",
        fontSize: 20,
        fill: 0xffffff,
        fontWeight: "bold",
        align: "left",
      },
    });
    title.x = -200;
    title.y = 20;
    content.addChild(title);

    // MSAA checkbox
    const msaaCheckbox = new Checkbox({
      label: "Anti-aliasing (MSAA)",
      checked: userSettings.getMsaaEnabled(),
      fontSize: 16,
    });
    msaaCheckbox.x = -200;
    msaaCheckbox.y = 60;
    msaaCheckbox.onChange.connect((checked) => {
      userSettings.setMsaaEnabled(checked);
    });
    content.addChild(msaaCheckbox);

    // Info text about requiring reload
    const infoText = new Text({
      text: "Note: Graphics settings require a page refresh to take effect.",
      style: {
        fontFamily: "Arial",
        fontSize: 14,
        fill: 0xaaaaaa,
        align: "left",
        wordWrap: true,
        wordWrapWidth: 400,
      },
    });
    infoText.x = -200;
    infoText.y = 100;
    content.addChild(infoText);

    this.contentContainer.addChild(content);
  }

  private createKeybindsContent(): void {
    const keybinds = this.keybindManager.getKeybinds();
    const actions: KeybindAction[] = [
      "thrust",
      "invThrust",
      "left",
      "right",
      "boost",
      "rcs",
      "drop",
      "shield",
      "map",
      "buildMode",
    ];

    // Create scrollable content container
    const scrollContent = new Container();

    // Add instruction text at top
    const instructionText = new Text({
      text: "Click a key button to rebind. Press ESC to cancel.",
      style: {
        fontFamily: "Arial",
        fontSize: 14,
        fill: 0xaaaaaa,
        align: "center",
      },
    });
    instructionText.x = -instructionText.width / 2 + 250;
    instructionText.y = 0;
    scrollContent.addChild(instructionText);

    let yOffset = 30;
    const rowHeight = 40;

    actions.forEach((action) => {
      const actionName = this.keybindManager.getActionDisplayName(action);
      const keys = keybinds[action];

      // Action label
      const label = new Text({
        text: actionName,
        style: {
          fontFamily: "Arial",
          fontSize: 16,
          fill: 0xffffff,
          align: "left",
        },
      });
      label.x = 0;
      label.y = yOffset;
      scrollContent.addChild(label);

      // Primary key button
      const primaryKeyText = keys[0]
        ? KeybindManager.formatKeyCode(keys[0])
        : "None";
      const primaryButton = new Button({
        text: primaryKeyText,
        width: 100,
        height: 30,
        fontSize: 14,
      });
      primaryButton.x = 200;
      primaryButton.y = yOffset - 5;
      primaryButton.onPress.connect(() => this.startRebinding(action, 0));
      scrollContent.addChild(primaryButton);

      // Secondary key button (if exists)
      if (keys.length > 1) {
        const secondaryKeyText = KeybindManager.formatKeyCode(keys[1]);
        const secondaryButton = new Button({
          text: secondaryKeyText,
          width: 100,
          height: 30,
          fontSize: 14,
        });
        secondaryButton.x = 310;
        secondaryButton.y = yOffset - 5;
        secondaryButton.onPress.connect(() => this.startRebinding(action, 1));
        scrollContent.addChild(secondaryButton);
      }

      yOffset += rowHeight;
    });

    // Reset to defaults button
    const resetButton = new Button({
      text: "Reset to Defaults",
      width: 150,
      height: 35,
      fontSize: 16,
    });
    resetButton.x = 125;
    resetButton.y = yOffset + 10;
    resetButton.onPress.connect(() => {
      this.keybindManager.resetToDefaults();
      this.switchTab("keybinds");
    });
    scrollContent.addChild(resetButton);

    // Calculate scrollbox height based on available space
    // Account for: title (70px), tabs (50px), padding (40px), total ~160px from top
    const scrollBoxHeight = this.menuHeight - 180;

    // Create ScrollBox
    const scrollBox = new ScrollBox({
      width: 500,
      height: scrollBoxHeight,
      items: [scrollContent],
    });

    scrollBox.x = -250;
    scrollBox.y = 0;

    this.contentContainer.addChild(scrollBox);
  }

  private startRebinding(action: KeybindAction, keyIndex: number): void {
    this.rebindingAction = action;
    this.rebindingKeyIndex = keyIndex;

    // Remove old handler if exists
    if (this.keydownHandler)
      document.removeEventListener("keydown", this.keydownHandler);

    // Create and add new handler
    this.keydownHandler = (e: KeyboardEvent) => {
      e.preventDefault();
      e.stopPropagation();

      // Ignore Escape - let user cancel rebinding
      if (e.code === "Escape") {
        this.stopRebinding();
        return;
      }

      if (this.rebindingAction !== null) {
        this.keybindManager.setKeybind(
          this.rebindingAction,
          this.rebindingKeyIndex,
          e.code,
        );
        this.stopRebinding();
        this.switchTab("keybinds"); // Refresh UI
      }
    };

    document.addEventListener("keydown", this.keydownHandler);
  }

  private stopRebinding(): void {
    if (this.keydownHandler) {
      document.removeEventListener("keydown", this.keydownHandler);
      this.keydownHandler = null;
    }
    this.rebindingAction = null;
    this.rebindingKeyIndex = 0;
  }

  public show(): void {
    this.visible = true;
    this.updateSize();
  }

  public hide(): void {
    this.visible = false;
    this.stopRebinding();
  }

  public isVisible(): boolean {
    return this.visible;
  }

  private updateSize(): void {
    // Recalculate menu height as 75% of new window height
    const newMenuHeight = Math.floor(window.innerHeight * 0.75);

    if (newMenuHeight !== this.menuHeight) {
      this.menuHeight = newMenuHeight;

      // Redraw menu background with new size
      this.menuBackground.clear();
      this.menuBackground.rect(
        -this.menuWidth / 2,
        -this.menuHeight / 2,
        this.menuWidth,
        this.menuHeight,
      );
      this.menuBackground.fill({ color: 0x2a2a2a, alpha: 0.95 });
      this.menuBackground.stroke({ width: 2, color: 0x555555 });

      // Refresh current tab to update scrollbox size
      const currentTab = this.generalTabButton.getPressed()
        ? "general"
        : "keybinds";
      this.switchTab(currentTab);
    }

    // Update overlay size to match current window size
    this.overlay.clear();
    this.overlay.rect(0, 0, window.innerWidth, window.innerHeight);
    this.overlay.fill({ color: 0x000000, alpha: 0.7 });

    // Re-center menu container
    this.menuContainer.x = window.innerWidth / 2;
    this.menuContainer.y = window.innerHeight / 2;
  }

  public resize(_width: number, _height: number): void {
    this.updateSize();
  }
}
