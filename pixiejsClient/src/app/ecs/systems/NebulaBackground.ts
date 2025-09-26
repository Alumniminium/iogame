import {
  Graphics,
  Texture,
  RenderTexture,
  Application,
  BlurFilter,
  Container,
  Sprite,
} from "pixi.js";

// Catppuccin Frappé color palette for nebula
export const CATPPUCCIN_FRAPPE = {
  base: 0x303446, // Dark base
  mantle: 0x292c3c, // Darker mantle
  crust: 0x232634, // Darkest crust
  surface0: 0x414559, // Surface colors
  surface1: 0x51576d,
  surface2: 0x626880,
  overlay0: 0x737994,
  overlay1: 0x838ba7,
  overlay2: 0x949cbb,
  subtext0: 0xa5adce,
  subtext1: 0xb5bfe2,
  text: 0xc6d0f5,

  // Accent colors for nebula clouds
  rosewater: 0xf2d5cf,
  flamingo: 0xeebebe,
  pink: 0xf4b8e4,
  mauve: 0xca9ee6,
  red: 0xe78284,
  maroon: 0xea999c,
  peach: 0xef9f76,
  yellow: 0xe5c890,
  green: 0xa6d189,
  teal: 0x81c8be,
  sky: 0x99d1db,
  sapphire: 0x85c1dc,
  blue: 0x8caaee,
  lavender: 0xbabbf1,
};

interface NoiseSettings {
  octaves: number;
  persistence: number;
  scale: number;
  offsetX: number;
  offsetY: number;
}

export class NebulaBackground {
  private width: number;
  private height: number;
  private cellSize: number = 256; // Size of each texture cell
  private textureCache: Map<string, RenderTexture> = new Map();
  private spriteGrid: Graphics | null = null;
  private lastGeneratedSize: { width: number; height: number } | null = null;
  private app: Application;

  constructor(width: number, height: number, app: Application) {
    this.width = width;
    this.height = height;
    this.app = app;
  }

  setApplication(app: Application): void {
    this.app = app;
  }

  public generateNebula(): Graphics {
    // Check if we can reuse cached graphics
    if (
      this.spriteGrid &&
      this.lastGeneratedSize &&
      this.lastGeneratedSize.width === this.width &&
      this.lastGeneratedSize.height === this.height
    ) {
      return this.spriteGrid;
    }

    // Create grid-based nebula
    this.spriteGrid = new Graphics();

    // Calculate grid dimensions
    const gridCols = Math.ceil(this.width / this.cellSize);
    const gridRows = Math.ceil(this.height / this.cellSize);

    // Generate texture for each grid cell
    for (let col = 0; col < gridCols; col++) {
      for (let row = 0; row < gridRows; row++) {
        const cellX = col * this.cellSize;
        const cellY = row * this.cellSize;
        const cellKey = `${col}-${row}`;

        // Check if we already have this cell cached
        let cellTexture = this.textureCache.get(cellKey);
        if (!cellTexture) {
          cellTexture = this.generateCellTexture(cellX, cellY);
          this.textureCache.set(cellKey, cellTexture);
        }

        // Create sprite for this cell
        const cellSprite = new Sprite(cellTexture);
        cellSprite.x = cellX;
        cellSprite.y = cellY;
        this.spriteGrid.addChild(cellSprite);
      }
    }

    this.lastGeneratedSize = { width: this.width, height: this.height };
    return this.spriteGrid;
  }

  private generateCellTexture(offsetX: number, offsetY: number): RenderTexture {
    // Create temporary container for this cell
    const tempContainer = new Container();
    const graphics = new Graphics();

    // Create base space background for this cell
    this.drawBaseCellSpace(graphics, offsetX, offsetY);

    // Create nebula layers for this specific cell region
    this.addBlurredCellLayer(
      tempContainer,
      CATPPUCCIN_FRAPPE.mauve,
      {
        octaves: 3,
        persistence: 0.5,
        scale: 0.003,
        offsetX: offsetX,
        offsetY: offsetY,
      },
      0.4,
    );
    this.addBlurredCellLayer(
      tempContainer,
      CATPPUCCIN_FRAPPE.blue,
      {
        octaves: 3,
        persistence: 0.6,
        scale: 0.002,
        offsetX: offsetX + 100,
        offsetY: offsetY + 50,
      },
      0.35,
    );
    this.addBlurredCellLayer(
      tempContainer,
      CATPPUCCIN_FRAPPE.teal,
      {
        octaves: 2,
        persistence: 0.7,
        scale: 0.0015,
        offsetX: offsetX + 200,
        offsetY: offsetY - 30,
      },
      0.3,
    );
    this.addBlurredCellLayer(
      tempContainer,
      CATPPUCCIN_FRAPPE.pink,
      {
        octaves: 2,
        persistence: 0.4,
        scale: 0.004,
        offsetX: offsetX - 50,
        offsetY: offsetY + 80,
      },
      0.25,
    );
    this.addBlurredCellLayer(
      tempContainer,
      CATPPUCCIN_FRAPPE.lavender,
      {
        octaves: 3,
        persistence: 0.5,
        scale: 0.0025,
        offsetX: offsetX - 80,
        offsetY: offsetY + 150,
      },
      0.3,
    );

    // Create render texture for this cell
    const cellTexture = RenderTexture.create({
      width: this.cellSize,
      height: this.cellSize,
    });

    // Render base space first
    this.app.renderer.render({ container: graphics, target: cellTexture });

    // Render nebula layers
    this.app.renderer.render({ container: tempContainer, target: cellTexture });

    // Clean up temporary objects
    tempContainer.destroy();
    graphics.destroy();

    return cellTexture;
  }

  private drawBaseCellSpace(
    graphics: Graphics,
    offsetX: number,
    offsetY: number,
  ): void {
    // Create base space background for this cell
    graphics
      .rect(0, 0, this.cellSize, this.cellSize)
      .fill(CATPPUCCIN_FRAPPE.crust);

    // Add stars for this cell region
    this.addCellStars(graphics, offsetX, offsetY);
  }

  private addCellStars(
    graphics: Graphics,
    offsetX: number,
    offsetY: number,
  ): void {
    const starCount = Math.floor((this.cellSize * this.cellSize) / 25000);

    // Use seeded random based on cell position for consistent star placement
    const seed = (offsetX * 73 + offsetY * 37) % 9999;

    for (let i = 0; i < starCount; i++) {
      // Seeded random for consistent placement
      const randSeed = (seed + i * 123) % 9999;
      const x = randSeed % this.cellSize;
      const y = (randSeed * 7) % this.cellSize;
      const brightness = (randSeed % 100) / 100;
      const size = (randSeed % 12) / 10 + 0.5;

      let color: number;
      if (brightness > 0.9) {
        color = CATPPUCCIN_FRAPPE.text;
      } else if (brightness > 0.7) {
        color = CATPPUCCIN_FRAPPE.subtext0;
      } else {
        color = CATPPUCCIN_FRAPPE.overlay1;
      }

      graphics.circle(x, y, size).fill({ color, alpha: brightness * 0.8 });
    }
  }

  private addStars(graphics: Graphics): void {
    const starCount = Math.floor((this.width * this.height) / 25000); // Better density for visual quality

    for (let i = 0; i < starCount; i++) {
      const x = Math.random() * this.width;
      const y = Math.random() * this.height;
      const brightness = Math.random();
      const size = Math.random() * 1.2 + 0.5; // Slightly larger stars

      let color: number;
      if (brightness > 0.9) {
        color = CATPPUCCIN_FRAPPE.text;
      } else if (brightness > 0.7) {
        color = CATPPUCCIN_FRAPPE.subtext0;
      } else {
        color = CATPPUCCIN_FRAPPE.overlay1;
      }

      graphics.circle(x, y, size).fill({ color, alpha: brightness * 0.8 });
    }
  }

  private addNebulaLayer(
    graphics: Graphics,
    color: number,
    noise: NoiseSettings,
    maxAlpha: number,
  ): void {
    const resolution = 6; // Reduced resolution by half
    const patchSize = resolution * 2; // Larger patches to cover gaps

    // Batch drawing operations for better performance
    const patches: { x: number; y: number; alpha: number }[] = [];

    for (let x = 0; x < this.width; x += resolution) {
      for (let y = 0; y < this.height; y += resolution) {
        const noiseValue = this.generateNoise(
          (x + noise.offsetX) * noise.scale,
          (y + noise.offsetY) * noise.scale,
          noise.octaves,
          noise.persistence,
        );

        // Use smooth falloff to create more organic cloud shapes
        const smoothed = this.smoothStep(noiseValue, 0.3, 0.8);
        const alpha = smoothed * maxAlpha;

        if (alpha > 0.02) {
          patches.push({ x, y, alpha });
        }
      }
    }

    // Draw all patches in batches with same alpha values
    const alphaGroups = new Map<number, { x: number; y: number }[]>();
    patches.forEach((patch) => {
      const roundedAlpha = Math.round(patch.alpha * 20) / 20; // Group similar alphas
      if (!alphaGroups.has(roundedAlpha)) {
        alphaGroups.set(roundedAlpha, []);
      }
      alphaGroups.get(roundedAlpha)!.push({ x: patch.x, y: patch.y });
    });

    // Draw each alpha group in one operation
    alphaGroups.forEach((positions, alpha) => {
      positions.forEach(({ x, y }) => {
        graphics.rect(x, y, patchSize, patchSize);
      });
      graphics.fill({ color, alpha });
    });
  }

  private addBlurredNebulaLayer(
    container: Container,
    color: number,
    noise: NoiseSettings,
    maxAlpha: number,
  ): void {
    const graphics = new Graphics();

    // Use higher resolution for better detail before blurring
    const resolution = 4; // Much higher resolution for quality
    const patchSize = resolution * 1.5;

    for (let x = 0; x < this.width; x += resolution) {
      for (let y = 0; y < this.height; y += resolution) {
        const noiseValue = this.generateNoise(
          (x + noise.offsetX) * noise.scale,
          (y + noise.offsetY) * noise.scale,
          noise.octaves,
          noise.persistence,
        );

        const smoothed = this.smoothStep(noiseValue, 0.2, 0.9);
        const alpha = smoothed * maxAlpha;

        if (alpha > 0.02) {
          graphics.rect(x, y, patchSize, patchSize);
          graphics.fill({ color, alpha });
        }
      }
    }

    // Apply optimized blur to each layer individually
    const optimizedBlur = new BlurFilter({
      strength: 25, // Reduced blur to preserve more detail
      quality: 10, // Higher quality blur
    });
    graphics.filters = [optimizedBlur];

    container.addChild(graphics);
  }

  private addBlurredCellLayer(
    container: Container,
    color: number,
    noise: NoiseSettings,
    maxAlpha: number,
  ): void {
    const graphics = new Graphics();

    // Higher resolution for cell-based generation
    const resolution = 3;
    const patchSize = resolution * 1.5;

    for (let x = 0; x < this.cellSize; x += resolution) {
      for (let y = 0; y < this.cellSize; y += resolution) {
        const worldX = x + noise.offsetX;
        const worldY = y + noise.offsetY;

        const noiseValue = this.generateNoise(
          worldX * noise.scale,
          worldY * noise.scale,
          noise.octaves,
          noise.persistence,
        );

        const smoothed = this.smoothStep(noiseValue, 0.2, 0.9);
        const alpha = smoothed * maxAlpha;

        if (alpha > 0.02) {
          graphics.rect(x, y, patchSize, patchSize);
          graphics.fill({ color, alpha });
        }
      }
    }

    // Apply optimized blur to each layer individually
    const optimizedBlur = new BlurFilter({
      strength: 25,
      quality: 10,
    });
    graphics.filters = [optimizedBlur];

    container.addChild(graphics);
  }

  private generateNoise(
    x: number,
    y: number,
    octaves: number,
    persistence: number,
  ): number {
    let value = 0;
    let amplitude = 1;
    let frequency = 1;
    let maxValue = 0;

    for (let i = 0; i < octaves; i++) {
      value += this.simpleNoise(x * frequency, y * frequency) * amplitude;
      maxValue += amplitude;
      amplitude *= persistence;
      frequency *= 2;
    }

    return value / maxValue;
  }

  private simpleNoise(x: number, y: number): number {
    // Simple pseudo-random noise function
    const n = Math.sin(x * 12.9898 + y * 78.233) * 43758.5453;
    return (n - Math.floor(n)) * 2 - 1; // Normalize to [-1, 1]
  }

  private smoothStep(value: number, edge0: number, edge1: number): number {
    const t = Math.max(0, Math.min(1, (value - edge0) / (edge1 - edge0)));
    return t * t * (3 - 2 * t);
  }

  private createGradient(stops: Array<{ color: number; stop: number }>): any {
    // Create a simple vertical gradient effect using multiple rectangles
    // This is a simplified gradient since PIXI.js gradients can be complex
    const baseColor = stops[0].color;

    // For now, return the base color - in a real implementation,
    // you'd create a proper gradient texture
    return baseColor;
  }

  public resize(width: number, height: number): void {
    this.width = width;
    this.height = height;
    // Invalidate cache when size changes
    this.clearCache();
  }

  public destroy(): void {
    this.clearCache();
  }

  private clearCache(): void {
    // Clear sprite grid
    if (this.spriteGrid) {
      this.spriteGrid.destroy();
      this.spriteGrid = null;
    }

    // Clear cached textures
    this.textureCache.forEach((texture) => {
      texture.destroy();
    });
    this.textureCache.clear();

    this.lastGeneratedSize = null;
  }
}
