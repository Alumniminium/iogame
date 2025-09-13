import { InputState } from '../../input/InputManager';

export interface InputOverlayConfig {
  canvas: HTMLCanvasElement;
  position: { x: number; y: number };
  size: { width: number; height: number };
}

export interface EntityStats {
  health?: { current: number; max: number };
  energy?: { current: number; max: number };
  engine?: { throttle: number; powerDraw: number; rcsActive: boolean };
}

export class InputOverlay {
  private canvas: HTMLCanvasElement;
  private ctx: CanvasRenderingContext2D;
  private position: { x: number; y: number };
  private size: { width: number; height: number };
  private visible = true;

  // Key binding labels for display
  private keyLabels = {
    thrust: 'W/↑ Thrust',
    invThrust: 'S/↓ Reverse',
    left: 'A/← Left',
    right: 'D/→ Right',
    boost: 'Shift Boost',
    rcs: 'R RCS (Toggle)',
    fire: 'Click Fire',
    drop: 'Q/E Drop',
    shield: 'Space Shield (Toggle)'
  };

  constructor(config: InputOverlayConfig) {
    this.canvas = config.canvas;
    this.ctx = this.canvas.getContext('2d')!;
    this.position = config.position;
    this.size = config.size;
  }

  render(inputState: InputState, entityStats?: EntityStats): void {
    if (!this.visible) return;

    this.ctx.save();

    // Set up drawing style
    this.ctx.font = '12px monospace';
    this.ctx.textAlign = 'left';
    this.ctx.textBaseline = 'top';

    // Calculate dynamic height based on content
    const baseHeight = 180;
    const statsHeight = entityStats ? 80 : 0;
    this.size.height = baseHeight + statsHeight;

    // Draw background
    this.drawBackground();

    // Draw title
    this.ctx.fillStyle = '#ffffff';
    this.ctx.fillText('INPUT STATE', this.position.x + 10, this.position.y + 10);

    let yOffset = 30;

    // Draw each input state
    Object.entries(this.keyLabels).forEach(([key, label]) => {
      const value = inputState[key as keyof InputState];
      const isActive = typeof value === 'boolean' ? value : false;
      this.drawInputState(label, isActive, yOffset);
      yOffset += 16;
    });

    // Draw mouse position
    this.ctx.fillStyle = '#cccccc';
    this.ctx.fillText(`Mouse: (${inputState.mouseX}, ${inputState.mouseY})`,
      this.position.x + 10, this.position.y + yOffset);
    yOffset += 16;

    // Draw movement vector
    this.ctx.fillText(`Move: (${inputState.moveX.toFixed(2)}, ${inputState.moveY.toFixed(2)})`,
      this.position.x + 10, this.position.y + yOffset);
    yOffset += 20;

    // Draw entity stats if available
    if (entityStats) {
      this.drawEntityStats(entityStats, yOffset);
    }

    this.ctx.restore();
  }

  private drawEntityStats(stats: EntityStats, yOffset: number): void {
    // Draw separator line
    this.ctx.strokeStyle = '#555555';
    this.ctx.lineWidth = 1;
    this.ctx.beginPath();
    this.ctx.moveTo(this.position.x + 10, this.position.y + yOffset - 5);
    this.ctx.lineTo(this.position.x + this.size.width - 10, this.position.y + yOffset - 5);
    this.ctx.stroke();

    // Draw stats title
    this.ctx.fillStyle = '#ffffff';
    this.ctx.fillText('PLAYER STATS', this.position.x + 10, this.position.y + yOffset + 5);
    yOffset += 25;

    // Health bar
    if (stats.health) {
      this.drawStatBar('Health', stats.health.current, stats.health.max, '#ff4444', yOffset);
      yOffset += 16;
    }

    // Energy bar
    if (stats.energy) {
      this.drawStatBar('Energy', stats.energy.current, stats.energy.max, '#44ff44', yOffset);
      yOffset += 16;
    }

    // Engine stats
    if (stats.engine) {
      this.ctx.fillStyle = '#cccccc';
      this.ctx.fillText(`Throttle: ${stats.engine.throttle}%`, this.position.x + 10, this.position.y + yOffset);
      yOffset += 14;

      this.ctx.fillText(`Power: ${stats.engine.powerDraw.toFixed(1)}kW`, this.position.x + 10, this.position.y + yOffset);
      yOffset += 14;

      if (stats.engine.rcsActive) {
        this.ctx.fillStyle = '#ffaa00';
        this.ctx.fillText('RCS ACTIVE', this.position.x + 10, this.position.y + yOffset);
        yOffset += 14;
      }

      // Show shield status from input state
      const inputState = (window as any).currentInputState;
      if (inputState?.shield) {
        this.ctx.fillStyle = '#00aaff';
        this.ctx.fillText('SHIELD ACTIVE', this.position.x + 10, this.position.y + yOffset);
      }
    }
  }

  private drawStatBar(label: string, current: number, max: number, color: string, yOffset: number): void {
    const barX = this.position.x + 10;
    const barY = this.position.y + yOffset;
    const barWidth = this.size.width - 20;
    const barHeight = 12;

    // Background
    this.ctx.fillStyle = '#333333';
    this.ctx.fillRect(barX, barY, barWidth, barHeight);

    // Filled portion
    const fillWidth = (current / max) * barWidth;
    this.ctx.fillStyle = color;
    this.ctx.fillRect(barX, barY, fillWidth, barHeight);

    // Text overlay
    this.ctx.fillStyle = '#ffffff';
    this.ctx.font = '10px monospace';
    const text = `${label}: ${Math.round(current)}/${Math.round(max)}`;
    const textWidth = this.ctx.measureText(text).width;
    const textX = barX + (barWidth - textWidth) / 2;
    this.ctx.fillText(text, textX, barY + 9);

    // Reset font
    this.ctx.font = '12px monospace';
  }

  private drawBackground(): void {
    this.ctx.fillStyle = 'rgba(0, 0, 0, 0.7)';
    this.ctx.fillRect(
      this.position.x,
      this.position.y,
      this.size.width,
      this.size.height
    );

    // Draw border
    this.ctx.strokeStyle = '#555555';
    this.ctx.lineWidth = 1;
    this.ctx.strokeRect(
      this.position.x,
      this.position.y,
      this.size.width,
      this.size.height
    );
  }

  private drawInputState(label: string, active: boolean, yOffset: number): void {
    // Draw indicator dot
    const dotX = this.position.x + 10;
    const dotY = this.position.y + yOffset + 6;

    this.ctx.fillStyle = active ? '#00ff00' : '#333333';
    this.ctx.beginPath();
    this.ctx.arc(dotX, dotY, 4, 0, Math.PI * 2);
    this.ctx.fill();

    // Draw label
    this.ctx.fillStyle = active ? '#ffffff' : '#888888';
    this.ctx.fillText(label, dotX + 12, this.position.y + yOffset);
  }

  setVisible(visible: boolean): void {
    this.visible = visible;
  }

  isVisible(): boolean {
    return this.visible;
  }

  toggle(): void {
    this.visible = !this.visible;
  }

  setPosition(x: number, y: number): void {
    this.position.x = x;
    this.position.y = y;
  }

  setSize(width: number, height: number): void {
    this.size.width = width;
    this.size.height = height;
  }
}