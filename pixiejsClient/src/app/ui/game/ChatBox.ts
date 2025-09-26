import { Container, Graphics, Text } from "pixi.js";

export interface ChatMessage {
  playerId: string;
  playerName: string;
  message: string;
  timestamp: number;
}

const defaultChatOptions = {
  width: 400,
  height: 300,
  maxMessages: 100,
  fontSize: 14,
  visible: true,
};

type ChatOptions = typeof defaultChatOptions;

export class ChatBox extends Container {
  private background!: Graphics;
  private messagesContainer!: Container;
  private inputBackground!: Graphics;
  private inputText!: Text;
  private messages: ChatMessage[] = [];
  private messageTexts: Text[] = [];
  private options: ChatOptions;

  private isTyping = false;
  private currentInput = "";
  private onSendCallback?: (message: string) => void;
  private keyDownHandler?: (e: KeyboardEvent) => void;

  constructor(options: Partial<ChatOptions> = {}) {
    super();

    this.options = { ...defaultChatOptions, ...options };
    this.visible = this.options.visible;

    this.createBackground();
    this.createMessagesArea();
    this.createInputArea();
    this.setupInteraction();
  }

  private createBackground(): void {
    this.background = new Graphics();
    this.background
      .rect(0, 0, this.options.width, this.options.height)
      .fill({ color: 0x000000, alpha: 0.7 })
      .stroke({ width: 1, color: 0x555555 });
    this.addChild(this.background);
  }

  private createMessagesArea(): void {
    this.messagesContainer = new Container();
    this.messagesContainer.x = 5;
    this.messagesContainer.y = 5;
    this.addChild(this.messagesContainer);
  }

  private createInputArea(): void {
    const inputHeight = 25;
    const inputY = this.options.height - inputHeight - 5;

    this.inputBackground = new Graphics();
    this.inputBackground
      .rect(5, inputY, this.options.width - 10, inputHeight)
      .fill({ color: 0x333333 })
      .stroke({ width: 1, color: 0x777777 });
    this.addChild(this.inputBackground);

    this.inputText = new Text({
      text: "",
      style: {
        fontFamily: "Arial",
        fontSize: this.options.fontSize,
        fill: 0xffffff,
      },
    });
    this.inputText.x = 10;
    this.inputText.y = inputY + 5;
    this.addChild(this.inputText);
  }

  private setupInteraction(): void {
    this.inputBackground.eventMode = "static";
    this.inputBackground.cursor = "text";

    this.inputBackground.on("pointerdown", () => {
      this.startTyping();
    });
  }

  public addMessage(
    playerId: string,
    playerName: string,
    message: string,
  ): void {
    const chatMessage: ChatMessage = {
      playerId,
      playerName,
      message,
      timestamp: Date.now(),
    };

    this.messages.push(chatMessage);

    if (this.messages.length > this.options.maxMessages) {
      this.messages.shift();
    }

    this.updateMessageDisplay();
  }

  private updateMessageDisplay(): void {
    this.messageTexts.forEach((text) => text.destroy());
    this.messageTexts = [];
    this.messagesContainer.removeChildren();

    const messageHeight = this.options.fontSize + 4;
    const maxVisibleMessages = Math.floor(
      (this.options.height - 40) / messageHeight,
    ); // -40 for input area

    const visibleMessages = this.messages.slice(-maxVisibleMessages);

    visibleMessages.forEach((msg, index) => {
      const messageText = this.createMessageText(msg);
      messageText.y = index * messageHeight;
      this.messagesContainer.addChild(messageText);
      this.messageTexts.push(messageText);
    });
  }

  private createMessageText(msg: ChatMessage): Text {
    let displayName = msg.playerName;
    let nameColor = 0xffffff;

    if (
      msg.playerId === "00000000-0000-0000-0000-000000000000" ||
      msg.playerId === ""
    ) {
      displayName = "[SERVER]";
      nameColor = 0x00ff00; // Green for server messages
    }

    const fullText = `${displayName}: ${msg.message}`;

    return new Text({
      text: fullText,
      style: {
        fontFamily: "Arial",
        fontSize: this.options.fontSize,
        fill: nameColor,
        wordWrap: true,
        wordWrapWidth: this.options.width - 20,
      },
    });
  }

  public startTyping(): void {
    if (this.isTyping) return;

    this.isTyping = true;
    this.currentInput = "";
    this.updateInputDisplay();

    this.inputBackground.clear();
    this.inputBackground
      .rect(5, this.options.height - 30, this.options.width - 10, 25)
      .fill({ color: 0x444444 })
      .stroke({ width: 2, color: 0x00ff00 });

    this.keyDownHandler = this.handleRealKeyDown.bind(this);
    window.addEventListener("keydown", this.keyDownHandler);
  }

  public stopTyping(): void {
    if (!this.isTyping) return;

    this.isTyping = false;
    this.currentInput = "";
    this.updateInputDisplay();

    this.inputBackground.clear();
    this.inputBackground
      .rect(5, this.options.height - 30, this.options.width - 10, 25)
      .fill({ color: 0x333333 })
      .stroke({ width: 1, color: 0x777777 });

    if (this.keyDownHandler) {
      window.removeEventListener("keydown", this.keyDownHandler);
      this.keyDownHandler = undefined;
    }
  }

  public handleKeyInput(key: string): boolean {
    if (!this.isTyping) return false;

    if (key === "Enter") {
      this.sendMessage();
      return true;
    } else if (key === "Escape") {
      this.stopTyping();
      return true;
    } else if (key === "Backspace") {
      if (this.currentInput.length > 0) {
        this.currentInput = this.currentInput.slice(0, -1);
        this.updateInputDisplay();
      }
      return true;
    } else if (key.length === 1 && this.currentInput.length < 100) {
      this.currentInput += key;
      this.updateInputDisplay();
      return true;
    }

    return false;
  }

  private handleRealKeyDown(e: KeyboardEvent): void {
    if (!this.isTyping) return;

    e.preventDefault();
    e.stopPropagation();

    if (e.key === "Enter") {
      this.sendMessage();
    } else if (e.key === "Escape") {
      this.stopTyping();
    } else if (e.key === "Backspace") {
      if (this.currentInput.length > 0) {
        this.currentInput = this.currentInput.slice(0, -1);
        this.updateInputDisplay();
      }
    } else if (e.key.length === 1 && this.currentInput.length < 100) {
      this.currentInput += e.key;
      this.updateInputDisplay();
    }
  }

  private updateInputDisplay(): void {
    const displayText = this.isTyping ? `> ${this.currentInput}|` : "";
    this.inputText.text = displayText;
  }

  private sendMessage(): void {
    if (this.currentInput.trim() && this.onSendCallback) {
      this.onSendCallback(this.currentInput.trim());
    }
    this.stopTyping();
  }

  public onSend(callback: (message: string) => void): void {
    this.onSendCallback = callback;
  }

  public toggle(): void {
    this.visible = !this.visible;
  }

  public show(): void {
    this.visible = true;
  }

  public hide(): void {
    this.visible = false;
    this.stopTyping();
  }

  public isCurrentlyTyping(): boolean {
    return this.isTyping;
  }

  public resize(screenWidth: number, screenHeight: number): void {
    this.x = screenWidth - this.options.width - 10;
    this.y = screenHeight - this.options.height - 10;
  }

  public destroy(): void {
    if (this.keyDownHandler) {
      window.removeEventListener("keydown", this.keyDownHandler);
    }
    super.destroy();
  }
}
