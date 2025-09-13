export interface BarData {
  current: number;
  max: number;
  label: string;
}

export interface StatusBarConfig {
  canvas: HTMLCanvasElement;
  position: { x: number; y: number };
  barWidth: number;
  barHeight: number;
  barSpacing: number;
  entityId?: number; // Optional entity ID for targeting
  title?: string; // Optional title (e.g., "Player", "Target", entity name)
  scale?: number; // Scale factor for bars (0.5-1.0 for smaller target bars)
}

export interface StatusBarData {
  health?: BarData;
  energy?: BarData;
  shield?: BarData;
  entityId: number;
  position?: { x: number; y: number }; // World position for floating bars
  title?: string;
}

export class StatusBars {
  private canvas: HTMLCanvasElement;
  private ctx: CanvasRenderingContext2D;
  private config: StatusBarConfig;

  constructor(config: StatusBarConfig) {
    this.canvas = config.canvas;
    this.ctx = this.canvas.getContext('2d')!;
    this.config = {
      scale: 1.0,
      ...config
    };
  }

  render(data: StatusBarData): void;
  render(health?: BarData, energy?: BarData, shield?: BarData): void;
  render(dataOrHealth?: StatusBarData | BarData, energy?: BarData, shield?: BarData): void {
    // Handle both overloads
    let health: BarData | undefined;
    let energyData: BarData | undefined;
    let shieldData: BarData | undefined;
    let renderPosition = this.config.position;
    let title: string | undefined;

    if (dataOrHealth && 'entityId' in dataOrHealth) {
      // StatusBarData overload
      const data = dataOrHealth as StatusBarData;
      health = data.health;
      energyData = data.energy;
      shieldData = data.shield;
      title = data.title;

      // Use world position if provided
      if (data.position) {
        renderPosition = data.position;
      }
    } else {
      // Individual parameters overload
      health = dataOrHealth as BarData | undefined;
      energyData = energy;
      shieldData = shield;
      title = this.config.title;
    }

    this.ctx.save();

    const scale = this.config.scale || 1.0;
    const scaledWidth = this.config.barWidth * scale;
    const scaledHeight = this.config.barHeight * scale;
    const scaledSpacing = this.config.barSpacing * scale;

    let yOffset = renderPosition.y;

    // Draw title if provided
    if (title) {
      this.ctx.fillStyle = '#ffffff';
      this.ctx.font = `${Math.round(12 * scale)}px monospace`;
      this.ctx.textAlign = 'left';
      this.ctx.fillText(title, renderPosition.x, yOffset - 5);
      yOffset += 15 * scale;
    }

    // Draw health bar
    if (health) {
      this.drawBar(
        health,
        { x: renderPosition.x, y: yOffset },
        '#ff4444', // Red for health
        '#ffffff', // White background
        scale
      );
      yOffset += scaledHeight + scaledSpacing;
    }

    // Draw energy/power bar
    if (energyData) {
      this.drawBar(
        energyData,
        { x: renderPosition.x, y: yOffset },
        '#44ff44', // Green for energy
        '#ffffff', // White background
        scale
      );
      yOffset += scaledHeight + scaledSpacing;
    }

    // Draw shield bar
    if (shieldData) {
      this.drawBar(
        shieldData,
        { x: renderPosition.x, y: yOffset },
        '#4444ff', // Blue for shield
        '#ffffff', // White background
        scale
      );
    }

    this.ctx.restore();
  }

  private drawBar(data: BarData, position: { x: number; y: number }, fillColor: string, bgColor: string, scale: number = 1.0): void {
    const x = position.x;
    const y = position.y;
    const width = this.config.barWidth * scale;
    const height = this.config.barHeight * scale;

    // Draw background
    this.ctx.fillStyle = bgColor;
    this.ctx.fillRect(x, y, width, height);

    // Calculate fill width
    const percent = Math.min(100, (data.current / data.max) * 100);
    const fillWidth = width * (percent / 100);

    // Draw filled portion
    this.ctx.fillStyle = fillColor;
    this.ctx.fillRect(x, y, fillWidth, height);

    // Draw border
    this.ctx.strokeStyle = '#000000';
    this.ctx.lineWidth = 1;
    this.ctx.strokeRect(x, y, width, height);

    // Draw text overlay
    this.ctx.fillStyle = '#000000';
    this.ctx.font = `${Math.round(14 * scale)}px monospace`;
    this.ctx.textAlign = 'left';
    this.ctx.textBaseline = 'middle';

    const text = `${data.label}: ${Math.round(data.current)} / ${Math.round(data.max)}`;
    this.ctx.fillText(text, x + 8 * scale, y + height / 2);
  }

  updateConfig(config: Partial<StatusBarConfig>): void {
    this.config = { ...this.config, ...config };
  }

  setPosition(x: number, y: number): void {
    this.config.position.x = x;
    this.config.position.y = y;
  }
}