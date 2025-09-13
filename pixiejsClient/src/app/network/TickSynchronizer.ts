export class TickSynchronizer {
  private static instance: TickSynchronizer | null = null;

  private serverTickOffset: number = 0;
  private clientStartTime: number = 0;
  private serverTicksPerSecond: number = 60; // Server runs at 60 TPS
  private synchronized: boolean = false;

  private constructor() {
    this.clientStartTime = Date.now();
  }

  static getInstance(): TickSynchronizer {
    if (!TickSynchronizer.instance) {
      TickSynchronizer.instance = new TickSynchronizer();
    }
    return TickSynchronizer.instance;
  }

  /**
   * Synchronize with server using TickCounter from LoginResponse
   */
  synchronizeWithServer(serverTick: number, latencyMs: number = 0): void {
    const clientTimeMs = Date.now() - this.clientStartTime;
    const clientTickEstimate = Math.floor(clientTimeMs / 1000 * this.serverTicksPerSecond);

    // Account for network latency - server tick was generated latencyMs ago
    const latencyTicks = Math.floor((latencyMs / 2) / 1000 * this.serverTicksPerSecond);
    const adjustedServerTick = serverTick + latencyTicks;

    this.serverTickOffset = adjustedServerTick - clientTickEstimate;
    this.synchronized = true;

    console.log(`Tick sync: server=${serverTick}, client=${clientTickEstimate}, offset=${this.serverTickOffset}, latency=${latencyMs}ms`);
  }

  /**
   * Get current server tick estimate
   */
  getCurrentServerTick(): number {
    if (!this.synchronized) {
      console.warn("TickSynchronizer not synchronized with server yet");
      return 0;
    }

    const clientTimeMs = Date.now() - this.clientStartTime;
    const clientTick = Math.floor(clientTimeMs / 1000 * this.serverTicksPerSecond);
    return clientTick + this.serverTickOffset;
  }

  /**
   * Convert client time to server tick
   */
  timeToServerTick(clientTimeMs: number): number {
    const clientTick = Math.floor((clientTimeMs - this.clientStartTime) / 1000 * this.serverTicksPerSecond);
    return clientTick + this.serverTickOffset;
  }

  /**
   * Convert server tick to client time
   */
  serverTickToTime(serverTick: number): number {
    const clientTick = serverTick - this.serverTickOffset;
    return this.clientStartTime + (clientTick / this.serverTicksPerSecond * 1000);
  }

  isSynchronized(): boolean {
    return this.synchronized;
  }

  reset(): void {
    this.synchronized = false;
    this.serverTickOffset = 0;
    this.clientStartTime = Date.now();
  }
}