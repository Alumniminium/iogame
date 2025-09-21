import { PacketHandler } from "./PacketHandler";
import { LoginRequestPacket } from './packets/LoginRequestPacket';
import { PingPacket } from './packets/PingPacket';

export interface NetworkConfig {
  serverUrl?: string;
  interpolationDelay?: number;
  predictionEnabled?: boolean;
}

export interface NetworkStats {
  connected: boolean;
  latency: number;
  bytesReceived: number;
  bytesSent: number;
  packetsReceived: number;
  packetsSent: number;
}

export class NetworkManager {
  private ws: WebSocket | null = null;
  private connected = false;
  private config: Required<NetworkConfig>;

  // Packet handling
  private packetHandler = new PacketHandler();

  // Timing
  private clientTime = 0;
  private lastPingTime = 0;
  private latency = 0;

  // Stats
  private bytesReceived = 0;
  private bytesSent = 0;
  private packetsReceived = 0;
  private packetsSent = 0;

  constructor(config: NetworkConfig = {}) {
    this.config = {
      serverUrl: config.serverUrl || "ws://localhost:5000/ws",
      interpolationDelay: config.interpolationDelay || 100,
      predictionEnabled: config.predictionEnabled !== false,
    };
  }


  async connect(playerName: string): Promise<boolean> {

    return new Promise((resolve) => {
      try {
        this.ws = new WebSocket(this.config.serverUrl);
        this.ws.binaryType = "arraybuffer";

        this.ws.onopen = () => {
          console.log("Connected to server");
          this.connected = true;
          // Send binary login request instead of JSON
          this.send(LoginRequestPacket.create(playerName, "pass123"));
          resolve(true);
        };

        this.ws.onmessage = (event) => {
          this.bytesReceived += event.data.byteLength;

          // Debug WebSocket message
          if (event.data.byteLength > 1000) {
            console.log(`Large WebSocket message: ${event.data.byteLength} bytes`);
          }

          // Add incoming data to buffer
          this.packetHandler.processPacket(new Uint8Array(event.data).buffer);
        };

        this.ws.onerror = (error) => {
          console.error("WebSocket error:", error);
          this.connected = false;
          resolve(false);
        };

        this.ws.onclose = () => {
          console.log("Disconnected from server");
          this.connected = false;
        };
      } catch (error) {
        console.error("Failed to connect:", error);
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

  getLatency(): number {
    return this.latency;
  }

  update(deltaTime: number): void {
    this.clientTime += deltaTime * 1000;

    // Send periodic ping
    const now = Date.now();
    if (now - this.lastPingTime > 1000) {
      if (this.connected && this.ws) {
        const packet = PingPacket.create();
        this.send(packet);
      }
      this.lastPingTime = now;
    }
  }


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
