import { PacketHandler } from "./PacketHandler";
import { LoginRequestPacket } from "./packets/LoginRequestPacket";
import { PingPacket } from "./packets/PingPacket";

/**
 * Network configuration options
 */
export interface NetworkConfig {
  serverUrl?: string;
  interpolationDelay?: number;
  predictionEnabled?: boolean;
}

/**
 * Network statistics for monitoring connection health
 */
export interface NetworkStats {
  connected: boolean;
  latency: number;
  bytesReceived: number;
  bytesSent: number;
  packetsReceived: number;
  packetsSent: number;
}

/**
 * Manages WebSocket connection to game server.
 * Handles packet sending/receiving, connection lifecycle, and latency tracking.
 * Implements singleton pattern.
 */
export class NetworkManager {
  private static instance: NetworkManager | null = null;

  private ws: WebSocket | null = null;
  private connected = false;
  private config: Required<NetworkConfig>;

  private packetHandler = new PacketHandler();

  private clientTime = 0;
  private lastPingTime = 0;
  private latency = 0;

  private bytesReceived = 0;
  private bytesSent = 0;
  private packetsReceived = 0;
  private packetsSent = 0;

  private constructor(config: NetworkConfig = {}) {
    this.config = {
      serverUrl: config.serverUrl || "ws://localhost:5000/ws",
      interpolationDelay: config.interpolationDelay || 100,
      predictionEnabled: config.predictionEnabled !== false,
    };
  }

  /**
   * Get or create the NetworkManager singleton
   */
  static getInstance(config?: NetworkConfig): NetworkManager {
    if (!NetworkManager.instance) {
      NetworkManager.instance = new NetworkManager(config);
    }
    return NetworkManager.instance;
  }

  /**
   * Get the existing instance without creating one
   */
  static getInstanceIfExists(): NetworkManager | null {
    return NetworkManager.instance;
  }

  /**
   * Send packet data if connected
   */
  static send(data: ArrayBuffer): void {
    if (NetworkManager.instance) NetworkManager.instance.send(data);
  }

  /**
   * Check if currently connected to server
   */
  static isConnected(): boolean {
    return NetworkManager.instance
      ? NetworkManager.instance.isConnected()
      : false;
  }

  /**
   * Update network state, send periodic pings
   */
  static update(deltaTime: number): void {
    NetworkManager.instance?.update(deltaTime);
  }

  /**
   * Connect to server with player name
   */
  static async connect(playerName: string): Promise<boolean> {
    if (!NetworkManager.instance) {
      return false;
    }
    return NetworkManager.instance.connect(playerName);
  }

  /**
   * Disconnect from server
   */
  static disconnect(): void {
    NetworkManager.instance?.disconnect();
  }

  /**
   * Establish WebSocket connection and send login packet
   */
  async connect(playerName: string): Promise<boolean> {
    return new Promise((resolve) => {
      try {
        this.ws = new WebSocket(this.config.serverUrl);
        this.ws.binaryType = "arraybuffer";

        this.ws.onopen = () => {
          this.connected = true;
          this.send(LoginRequestPacket.create(playerName, "pass123"));
          resolve(true);
        };

        this.ws.onmessage = (event) => {
          this.bytesReceived += event.data.byteLength;

          try {
            this.packetHandler.processPacket(new Uint8Array(event.data).buffer);
          } catch (error) {
            console.error("[NetworkManager] Error processing packet:", error);
          }
        };

        this.ws.onerror = (_error) => {
          this.connected = false;
          resolve(false);
        };

        this.ws.onclose = () => {
          this.connected = false;
        };
      } catch (error) {
        resolve(false);
      }
    });
  }

  disconnect(): void {
    if (this.ws) {
      this.ws.close();
      this.ws = null;
    }
    this.connected = false;
  }

  isConnected(): boolean {
    return this.connected;
  }

  /**
   * Get current network latency in milliseconds
   */
  getLatency(): number {
    return this.latency;
  }

  update(deltaTime: number): void {
    this.clientTime += deltaTime * 1000;

    const now = Date.now();
    if (now - this.lastPingTime > 1000) {
      if (this.connected && this.ws) {
        const packet = PingPacket.create();
        this.send(packet);
      }
      this.lastPingTime = now;
    }
  }

  /**
   * Get network statistics
   */
  getStats(): NetworkStats {
    return {
      connected: this.connected,
      latency: this.latency,
      bytesReceived: this.bytesReceived,
      bytesSent: this.bytesSent,
      packetsReceived: this.packetsReceived,
      packetsSent: this.packetsSent,
    };
  }

  send(data: ArrayBuffer): void {
    if (this.ws && this.ws.readyState === WebSocket.OPEN) {
      this.ws.send(data);
      this.bytesSent += data.byteLength;
      this.packetsSent++;
    }
  }
}
