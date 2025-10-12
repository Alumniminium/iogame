import { World } from "../ecs/core/World";
import { Container } from "pixi.js";
import { Vector2 } from "../ecs/core/types";
import { Camera as CameraImpl, CameraState } from "../ecs/Camera";

/**
 * Camera viewport representation for world-to-screen transformations
 */
export type Camera = CameraState;

/**
 * Manages camera position, zoom, and coordinate transformations.
 * Provides camera services for rendering and UI systems.
 */
export class CameraManager {
  private gameContainer: Container;
  private camera: CameraImpl;
  private viewDistance: number = 300;
  private canvasWidth = 800;
  private canvasHeight = 600;

  constructor(gameContainer: Container) {
    this.gameContainer = gameContainer;
    this.camera = new CameraImpl(1);
  }

  /**
   * Update canvas dimensions and recalculate zoom
   */
  resize(width: number, height: number): void {
    this.canvasWidth = width;
    this.canvasHeight = height;
    this.updateZoomFromViewDistance();
  }

  /**
   * Set view distance and update zoom accordingly
   */
  setViewDistance(viewDistance: number): void {
    this.viewDistance = viewDistance;
    this.updateZoomFromViewDistance();
  }

  private updateZoomFromViewDistance(): void {
    const fieldOfView = Math.PI / 4; // 45 degrees
    const distance = this.viewDistance;

    const viewportWidth = distance * Math.tan(fieldOfView);

    const zoom = this.canvasWidth / viewportWidth;
    this.camera.setZoom(zoom);
  }

  /**
   * Get a copy of the current camera state
   */
  getCamera(): Camera {
    return this.camera.getState();
  }

  /**
   * Update camera position and apply transforms
   */
  update(deltaTime: number): void {
    if (!World.Me) return;

    this.camera.setTarget(World.Me);
    this.camera.update(deltaTime);
    this.applyCamera();
  }

  private applyCamera(): void {
    const centerX = this.canvasWidth / 2;
    const centerY = this.canvasHeight / 2;
    const cameraState = this.camera.getState();

    this.gameContainer.pivot.set(cameraState.x, cameraState.y);
    this.gameContainer.position.set(centerX, centerY);
    this.gameContainer.scale.set(cameraState.zoom);
    this.gameContainer.rotation = cameraState.rotation;
  }

  /**
   * Convert world coordinates to screen coordinates
   */
  worldToScreen(worldX: number, worldY: number): Vector2 {
    const centerX = this.canvasWidth / 2;
    const centerY = this.canvasHeight / 2;
    const cameraState = this.camera.getState();

    const relativeX = worldX - cameraState.x;
    const relativeY = worldY - cameraState.y;

    const cos = Math.cos(cameraState.rotation);
    const sin = Math.sin(cameraState.rotation);
    const rotatedX = relativeX * cos - relativeY * sin;
    const rotatedY = relativeX * sin + relativeY * cos;

    return {
      x: centerX + rotatedX * cameraState.zoom,
      y: centerY + rotatedY * cameraState.zoom,
    };
  }

  /**
   * Convert screen coordinates to world coordinates
   */
  screenToWorld(screenX: number, screenY: number): Vector2 {
    const centerX = this.canvasWidth / 2;
    const centerY = this.canvasHeight / 2;
    const cameraState = this.camera.getState();

    const relativeX = (screenX - centerX) / cameraState.zoom;
    const relativeY = (screenY - centerY) / cameraState.zoom;

    const cos = Math.cos(-cameraState.rotation);
    const sin = Math.sin(-cameraState.rotation);
    const unrotatedX = relativeX * cos - relativeY * sin;
    const unrotatedY = relativeX * sin + relativeY * cos;

    return {
      x: cameraState.x + unrotatedX,
      y: cameraState.y + unrotatedY,
    };
  }
}
