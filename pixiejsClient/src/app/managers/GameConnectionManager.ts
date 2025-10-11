import { NetworkManager } from "../network/NetworkManager";

/**
 * Manages the game's network connection lifecycle.
 * Handles connection establishment and monitors connection state changes.
 */
export class GameConnectionManager {
  private networkManager: NetworkManager;
  private onConnectionEstablished: () => void;
  private onConnectionStateChange?: (connected: boolean) => void;

  constructor(networkManager: NetworkManager, onConnectionEstablished: () => void, onConnectionStateChange?: (connected: boolean) => void) {
    this.networkManager = networkManager;
    this.onConnectionEstablished = onConnectionEstablished;
    this.onConnectionStateChange = onConnectionStateChange;
  }

  /**
   * Start the connection process with a delay.
   * @param playerName - The player's name for authentication
   * @param delay - Delay in milliseconds before connecting (default: 100ms)
   */
  async startConnection(playerName: string, delay: number = 100): Promise<void> {
    setTimeout(async () => {
      try {
        const connected = await this.networkManager.connect(playerName);

        if (connected) {
          this.onConnectionEstablished();
        }
      } catch (error: unknown) {
        // Connection failed silently
      }
    }, delay);
  }

  /**
   * Monitor connection state changes and trigger callbacks.
   * Checks connection state periodically.
   * @param checkInterval - Interval in milliseconds (default: 100ms)
   */
  monitorConnectionState(checkInterval: number = 100): void {
    let wasConnected = this.networkManager.isConnected();

    setInterval(() => {
      const isConnected = this.networkManager.isConnected();
      if (isConnected !== wasConnected) {
        wasConnected = isConnected;

        this.onConnectionStateChange?.(isConnected);
      }
    }, checkInterval);
  }

  /**
   * Get the network manager instance.
   */
  getNetworkManager(): NetworkManager {
    return this.networkManager;
  }

  /**
   * Check if currently connected.
   */
  isConnected(): boolean {
    return this.networkManager.isConnected();
  }
}
