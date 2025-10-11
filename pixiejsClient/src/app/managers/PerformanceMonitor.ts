/**
 * Monitors game performance metrics including FPS and frame timing.
 * Manages fixed timestep accumulator for consistent physics updates.
 */
export class PerformanceMonitor {
  private fps: number = 0;
  private frameCount: number = 0;
  private lastFpsUpdate: number = 0;
  private lastDeltaMs: number = 0;
  private lastTime: number = 0;
  private accumulator: number = 0;

  private readonly fixedTimeStep: number = 1 / 60;
  private readonly maxAccumulator: number = 0.1;

  constructor() {
    this.lastTime = performance.now();
    this.lastFpsUpdate = this.lastTime;
  }

  /**
   * Update timing and accumulator. Should be called once per frame.
   * @returns Object containing deltaTime, lastDeltaMs, and whether to run fixed updates
   */
  update(): {
    deltaTime: number;
    lastDeltaMs: number;
    shouldRunFixedUpdate: boolean;
    fixedTimeStep: number;
  } {
    const currentTime = performance.now();
    const deltaTime = Math.min((currentTime - this.lastTime) / 1000, this.maxAccumulator);
    this.lastDeltaMs = currentTime - this.lastTime;
    this.lastTime = currentTime;

    this.updateFPS(currentTime);

    this.accumulator += deltaTime;

    return {
      deltaTime,
      lastDeltaMs: this.lastDeltaMs,
      shouldRunFixedUpdate: this.accumulator >= this.fixedTimeStep,
      fixedTimeStep: this.fixedTimeStep,
    };
  }

  /**
   * Consume one fixed timestep from the accumulator.
   * Should be called after each fixed update iteration.
   */
  consumeFixedTimestep(): void {
    this.accumulator -= this.fixedTimeStep;
  }

  /**
   * Check if another fixed update should run.
   */
  shouldRunFixedUpdate(): boolean {
    return this.accumulator >= this.fixedTimeStep;
  }

  /**
   * Get the current FPS.
   */
  getFPS(): number {
    return this.fps;
  }

  /**
   * Get the last frame delta time in milliseconds.
   */
  getLastDeltaMs(): number {
    return this.lastDeltaMs;
  }

  /**
   * Get the fixed timestep value.
   */
  getFixedTimeStep(): number {
    return this.fixedTimeStep;
  }

  /**
   * Reset timing state (e.g., when resuming from pause).
   */
  reset(): void {
    this.lastTime = performance.now();
    this.accumulator = 0;
  }

  private updateFPS(currentTime: number): void {
    this.frameCount++;

    if (currentTime - this.lastFpsUpdate >= 1000) {
      this.fps = this.frameCount;
      this.frameCount = 0;
      this.lastFpsUpdate = currentTime;
    }
  }
}
