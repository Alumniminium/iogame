export class PlayerNameManager {
  private static instance: PlayerNameManager;
  private playerNames: Map<string, string> = new Map();

  private constructor() {}

  public static getInstance(): PlayerNameManager {
    if (!PlayerNameManager.instance) {
      PlayerNameManager.instance = new PlayerNameManager();
    }
    return PlayerNameManager.instance;
  }

  public setPlayerName(playerId: string, name: string): void {
    this.playerNames.set(playerId, name);
    console.log(`📝 Player name registered: ${playerId} -> "${name}"`);
  }

  public getPlayerName(playerId: string): string {
    // Handle empty GUID or empty string as server messages
    if (
      playerId === "00000000-0000-0000-0000-000000000000" ||
      playerId === ""
    ) {
      return "[SERVER]";
    }

    return this.playerNames.get(playerId) || `Player-${playerId.slice(0, 8)}`;
  }

  public hasPlayerName(playerId: string): boolean {
    return this.playerNames.has(playerId);
  }

  public getAllPlayers(): Map<string, string> {
    return new Map(this.playerNames);
  }

  public clear(): void {
    this.playerNames.clear();
  }
}
