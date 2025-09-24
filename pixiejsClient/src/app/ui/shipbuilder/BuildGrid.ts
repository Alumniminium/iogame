import { Container, Graphics } from "pixi.js";

export interface GridPart {
  gridX: number;
  gridY: number;
  type: "hull" | "shield" | "engine";
  shape: "triangle" | "square";
  color: number;
  rotation: number; // 0=0°, 1=90°, 2=180°, 3=270°
}

export interface GridConfig {
  cellSize?: number;
  gridWidth?: number;
  gridHeight?: number;
  lineColor?: number;
  lineAlpha?: number;
  backgroundColor?: number;
  backgroundAlpha?: number;
}

export class BuildGrid extends Container {
  private cellSize: number;
  private gridWidth: number;
  private gridHeight: number;
  private gridGraphics: Graphics;
  private backgroundGraphics: Graphics;
  private highlightGraphics: Graphics;
  private partsContainer: Container;
  private ghostGraphics: Graphics;
  private placedParts = new Map<string, GridPart>();

  constructor(config: GridConfig = {}) {
    super();

    this.cellSize = config.cellSize || 32;
    this.gridWidth = config.gridWidth || 20;
    this.gridHeight = config.gridHeight || 15;

    this.backgroundGraphics = new Graphics();
    this.gridGraphics = new Graphics();
    this.partsContainer = new Container();
    this.ghostGraphics = new Graphics();
    this.highlightGraphics = new Graphics();

    this.addChild(this.backgroundGraphics);
    this.addChild(this.gridGraphics);
    this.addChild(this.partsContainer);
    this.addChild(this.ghostGraphics);
    this.addChild(this.highlightGraphics);

    this.drawGrid(config);
  }

  private drawGrid(config: GridConfig): void {
    const lineColor = config.lineColor || 0x444444;
    const lineAlpha = config.lineAlpha || 0.7;
    const backgroundColor = config.backgroundColor || 0x1a1a1a;
    const backgroundAlpha = config.backgroundAlpha || 0.8;

    this.backgroundGraphics.clear();
    this.backgroundGraphics
      .rect(
        0,
        0,
        this.gridWidth * this.cellSize,
        this.gridHeight * this.cellSize,
      )
      .fill({ color: backgroundColor, alpha: backgroundAlpha });

    this.gridGraphics.clear();

    for (let x = 0; x <= this.gridWidth; x++) {
      const xPos = x * this.cellSize;
      this.gridGraphics
        .moveTo(xPos, 0)
        .lineTo(xPos, this.gridHeight * this.cellSize)
        .stroke({ width: 0.1, color: lineColor, alpha: lineAlpha });
    }

    for (let y = 0; y <= this.gridHeight; y++) {
      const yPos = y * this.cellSize;
      this.gridGraphics
        .moveTo(0, yPos)
        .lineTo(this.gridWidth * this.cellSize, yPos)
        .stroke({ width: 0.1, color: lineColor, alpha: lineAlpha });
    }
  }

  worldToGrid(
    worldX: number,
    worldY: number,
  ): { gridX: number; gridY: number } {
    const localPoint = this.toLocal({ x: worldX, y: worldY });
    return {
      gridX: Math.floor(localPoint.x / this.cellSize),
      gridY: Math.floor(localPoint.y / this.cellSize),
    };
  }

  gridToWorld(
    gridX: number,
    gridY: number,
  ): { worldX: number; worldY: number } {
    const localX = gridX * this.cellSize + this.cellSize / 2;
    const localY = gridY * this.cellSize + this.cellSize / 2;
    const worldPoint = this.toGlobal({ x: localX, y: localY });
    return { worldX: worldPoint.x, worldY: worldPoint.y };
  }

  isValidGridPosition(gridX: number, gridY: number): boolean {
    return (
      gridX >= 0 &&
      gridX < this.gridWidth &&
      gridY >= 0 &&
      gridY < this.gridHeight
    );
  }

  highlightCell(
    gridX: number,
    gridY: number,
    color: number = 0x00ff00,
    alpha: number = 0.3,
  ): void {
    this.highlightGraphics.clear();

    if (this.isValidGridPosition(gridX, gridY)) {
      const x = gridX * this.cellSize;
      const y = gridY * this.cellSize;

      this.highlightGraphics
        .rect(x, y, this.cellSize, this.cellSize)
        .fill({ color, alpha });
    }
  }

  clearHighlight(): void {
    this.highlightGraphics.clear();
  }

  getCellSize(): number {
    return this.cellSize;
  }

  getGridDimensions(): { width: number; height: number } {
    return { width: this.gridWidth, height: this.gridHeight };
  }

  addPart(part: GridPart): void {
    const gridKey = `${part.gridX},${part.gridY}`;
    this.placedParts.set(gridKey, part);
    this.renderPart();
  }

  removePart(gridX: number, gridY: number): boolean {
    const gridKey = `${gridX},${gridY}`;
    if (this.placedParts.has(gridKey)) {
      this.placedParts.delete(gridKey);
      this.redrawAllParts();
      return true;
    }
    return false;
  }

  getPartAt(gridX: number, gridY: number): GridPart | null {
    const gridKey = `${gridX},${gridY}`;
    return this.placedParts.get(gridKey) || null;
  }

  clearAllParts(): void {
    this.placedParts.clear();
    this.partsContainer.removeChildren();
  }

  getAllParts(): GridPart[] {
    return Array.from(this.placedParts.values());
  }

  showGhost(
    gridX: number,
    gridY: number,
    type: string,
    shape: string,
    rotation: number = 0,
  ): void {
    this.ghostGraphics.clear();

    if (!this.isValidGridPosition(gridX, gridY)) return;

    const color = this.getPartColor(type);
    const x = gridX * this.cellSize + this.cellSize / 2;
    const y = gridY * this.cellSize + this.cellSize / 2;

    this.drawShape(this.ghostGraphics, shape, color, x, y, 0.5, rotation, type);
  }

  hideGhost(): void {
    this.ghostGraphics.clear();
  }

  private renderPart(): void {
    this.redrawAllParts();
  }

  private redrawAllParts(): void {
    this.partsContainer.removeChildren();

    for (const part of this.placedParts.values()) {
      const x = part.gridX * this.cellSize + this.cellSize / 2;
      const y = part.gridY * this.cellSize + this.cellSize / 2;

      const partGraphics = new Graphics();
      this.drawShape(
        partGraphics,
        part.shape,
        part.color,
        x,
        y,
        1.0,
        part.rotation,
        part.type,
      );
      this.partsContainer.addChild(partGraphics);
    }
  }

  private drawShape(
    graphics: Graphics,
    shape: string,
    color: number,
    x: number,
    y: number,
    alpha: number,
    rotation: number = 0,
    partType: string = "",
  ): void {
    const size = this.cellSize * 0.9; // Made bigger to reduce spacing

    graphics.alpha = alpha;

    const rotationRadians = (rotation * Math.PI) / 2;

    if (shape === "triangle") {
      const height = size * 0.866;

      const points = [
        { x: 0, y: -height / 2 }, // Top
        { x: -size / 2, y: height / 2 }, // Bottom left
        { x: size / 2, y: height / 2 }, // Bottom right
      ];

      const rotatedPoints: number[] = [];
      for (const point of points) {
        const cos = Math.cos(rotationRadians);
        const sin = Math.sin(rotationRadians);
        const rotX = point.x * cos - point.y * sin;
        const rotY = point.x * sin + point.y * cos;
        rotatedPoints.push(x + rotX, y + rotY);
      }

      graphics.poly(rotatedPoints).fill(color);
    } else {
      graphics.angle = rotation * 90; // Convert to degrees for PixiJS
      const halfSize = size / 2;
      graphics.rect(x - halfSize, y - halfSize, size, size).fill(color);
      graphics.angle = 0; // Reset rotation
    }

    if (partType === "engine") {
      this.drawEngineNozzle(graphics, x, y, rotation, size);
    }
  }

  private drawEngineNozzle(
    graphics: Graphics,
    engineX: number,
    engineY: number,
    rotation: number,
    gridSize: number,
  ): void {
    const engineRotation = (rotation * Math.PI) / 2;

    const exhaustDirection = engineRotation + Math.PI; // Add 180 degrees

    const nozzleLength = gridSize * 0.25;
    const nozzleWidth = gridSize * 0.12;

    const nozzleDistance = gridSize * 0.35;
    const nozzleCenterX = engineX + Math.cos(exhaustDirection) * nozzleDistance;
    const nozzleCenterY = engineY + Math.sin(exhaustDirection) * nozzleDistance;

    const nozzlePoints = [
      nozzleCenterX + Math.cos(exhaustDirection) * nozzleLength,
      nozzleCenterY + Math.sin(exhaustDirection) * nozzleLength,

      nozzleCenterX -
        Math.cos(exhaustDirection) * nozzleLength * 0.3 +
        Math.cos(exhaustDirection + Math.PI / 2) * nozzleWidth,
      nozzleCenterY -
        Math.sin(exhaustDirection) * nozzleLength * 0.3 +
        Math.sin(exhaustDirection + Math.PI / 2) * nozzleWidth,

      nozzleCenterX -
        Math.cos(exhaustDirection) * nozzleLength * 0.3 +
        Math.cos(exhaustDirection - Math.PI / 2) * nozzleWidth,
      nozzleCenterY -
        Math.sin(exhaustDirection) * nozzleLength * 0.3 +
        Math.sin(exhaustDirection - Math.PI / 2) * nozzleWidth,
    ];

    const nozzleColor = 0xcc4400; // Darker orange
    graphics.poly(nozzlePoints).fill(nozzleColor);
  }

  private getPartColor(type: string): number {
    switch (type) {
      case "hull":
        return 0x808080;
      case "shield":
        return 0x0080ff;
      case "engine":
        return 0xff8000;
      default:
        return 0xffffff;
    }
  }
}
