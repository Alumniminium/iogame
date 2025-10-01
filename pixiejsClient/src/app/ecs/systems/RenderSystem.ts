import { System } from "../core/System";
import { Entity } from "../core/Entity";
import { World } from "../core/World";
import { Box2DBodyComponent } from "../components/Box2DBodyComponent";
import { RenderComponent } from "../components/RenderComponent";
import { Container } from "pixi.js";
import { Vector2 } from "../core/types";
import { Camera as CameraImpl, CameraState } from "../Camera";
import { BackgroundRenderer } from "./renderers/BackgroundRenderer";
import { EntityRenderer } from "./renderers/EntityRenderer";
import { ShipPartRenderer } from "./renderers/ShipPartRenderer";
import { ShieldRenderer } from "./renderers/ShieldRenderer";
import { ParticleRenderer } from "./renderers/ParticleRenderer";
import { EffectRenderer } from "./renderers/EffectRenderer";

/**
 * Camera viewport representation for world-to-screen transformations
 */
export type Camera = CameraState;

/**
 * Coordinates rendering using specialized renderer components.
 * Handles camera control and delegates rendering tasks to specialized renderers.
 */
export class RenderSystem extends System {
  readonly componentTypes = [Box2DBodyComponent, RenderComponent];

  private gameContainer: Container;
  private camera: CameraImpl;
  private viewDistance: number = 300;
  private canvasWidth = 800;
  private canvasHeight = 600;

  // Specialized renderers
  private backgroundRenderer: BackgroundRenderer;
  private entityRenderer: EntityRenderer;
  private shipPartRenderer: ShipPartRenderer;
  private shieldRenderer: ShieldRenderer;
  private particleRenderer: ParticleRenderer;
  private effectRenderer: EffectRenderer;

  constructor(gameContainer: Container, _app: any) {
    super();
    this.gameContainer = gameContainer;
    this.camera = new CameraImpl(1);

    // Initialize specialized renderers
    this.backgroundRenderer = new BackgroundRenderer(gameContainer);
    this.shipPartRenderer = new ShipPartRenderer();
    this.entityRenderer = new EntityRenderer(
      gameContainer,
      this.shipPartRenderer,
    );
    this.shieldRenderer = new ShieldRenderer(gameContainer);
    this.particleRenderer = new ParticleRenderer(gameContainer);
    this.effectRenderer = new EffectRenderer(gameContainer);
  }

  initialize(): void {
    this.backgroundRenderer.initialize();
    this.entityRenderer.initialize();
    this.shipPartRenderer.initialize();
    this.shieldRenderer.initialize();
    this.particleRenderer.initialize();
    this.effectRenderer.initialize();
  }

  cleanup(): void {
    this.backgroundRenderer.cleanup();
    this.entityRenderer.cleanup();
    this.shipPartRenderer.cleanup();
    this.shieldRenderer.cleanup();
    this.particleRenderer.cleanup();
    this.effectRenderer.cleanup();
  }

  /**
   * Update canvas dimensions and recalculate zoom
   */
  resize(width: number, height: number): void {
    this.canvasWidth = width;
    this.canvasHeight = height;
    this.updateZoomFromViewDistance();
    this.backgroundRenderer.updateGrid(this.viewDistance);
  }

  /**
   * Set view distance and update zoom accordingly
   */
  setViewDistance(viewDistance: number): void {
    this.viewDistance = viewDistance;
    this.updateZoomFromViewDistance();
    this.backgroundRenderer.updateGrid(this.viewDistance);
  }

  /**
   * Set world map dimensions for background grid rendering
   */
  setMapSize(width: number, height: number): void {
    this.backgroundRenderer.setMapSize(width, height);
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

  update(deltaTime: number): void {
    super.update(deltaTime);

    const localPlayerId = (window as any).localPlayerId;
    if (!localPlayerId) return;

    const playerEntity = World.getEntity(localPlayerId);
    if (!playerEntity) return;

    this.camera.setTarget(playerEntity);
    this.camera.update(deltaTime);

    // Update all specialized renderers
    this.backgroundRenderer.update(deltaTime);
    this.entityRenderer.update(deltaTime);
    this.shipPartRenderer.update(deltaTime);
    this.shieldRenderer.update(deltaTime);
    this.particleRenderer.update(deltaTime);
    this.effectRenderer.update(deltaTime);

    this.applyCamera();
  }

  protected updateEntity(_entity: Entity, _deltaTime: number): void {
    // Entity rendering is now handled by specialized renderers
    // This method is kept for System compatibility but does nothing
  }

  /**
   * Get the entity ID currently under the mouse cursor
   */
  getHoveredEntityId(): string | null {
    return this.entityRenderer.getHoveredEntityId();
  }

  /**
   * Set the local player entity ID for special rendering
   */
  setLocalPlayerId(playerId: string | null): void {
    this.entityRenderer.setLocalPlayerId(playerId);
    this.shipPartRenderer.setLocalPlayerId(playerId);
  }

  /**
   * Toggle build mode which affects entity interactivity
   */
  setBuildModeActive(active: boolean): void {
    this.entityRenderer.setBuildModeActive(active);
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

  onEntityDestroyed(entity: Entity): void {
    this.entityRenderer.onEntityDestroyed(entity);
    this.shieldRenderer.onEntityDestroyed(entity);
    this.particleRenderer.onEntityDestroyed(entity);
    this.effectRenderer.onEntityDestroyed(entity);
  }
}
