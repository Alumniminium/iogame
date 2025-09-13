# iogame
player vs ai, left to right rush, horde

## Development Setup

### Prerequisites
- .NET 9 SDK
- Node.js 18+
- VS Code with C# and TypeScript extensions

### Quick Start

#### Option 1: VS Code Debug (Recommended)
1. Open the project in VS Code
2. Press `F5` or go to Run & Debug panel
3. Select **"🚀 FULL DEBUG (Server + Frontend + Client)"** for complete development setup
4. This will start everything you need with debugging enabled

#### Option 2: Command Line
```bash
# Install dependencies
npm install

# Start both server and client
npm run dev

# Or run individually
npm run server    # Starts .NET server on port 5000
npm run client    # Starts frontend dev server on port 3000
```

### Debug Configurations

Available in VS Code Run & Debug panel (`F5`):

- **🚀 FULL DEBUG (Server + Frontend + Client)**: Complete debugging setup
  - Starts .NET server with breakpoints
  - Starts frontend dev server with hot reload
  - Opens browser with debugging enabled
  - All running in parallel

- **⚡ QUICK DEBUG (Frontend + Client)**: Frontend-only debugging
  - Starts frontend dev server
  - Opens browser for client debugging

- **🔧 SERVER ONLY**: Debug .NET backend only
- **🎮 FRONTEND ONLY**: Debug TypeScript frontend only

### Manual Debug Script

Alternatively, run the debug script:

```bash
./start-debug.sh
```

This will:
1. Start the .NET server
2. Start the frontend dev server
3. Open your browser to http://localhost:3000
4. Keep all processes running until you press Ctrl+C

### Architecture

- **Backend**: .NET 9 with ECS (Entity Component System)
- **Frontend**: TypeScript with ECS mirroring backend architecture
- **Communication**: WebSocket connection between client and server

### Ports
- Server: http://localhost:5000
- Frontend: http://localhost:3000

Currently on hold as I'm focusing on a singleplayer spinoff for the RG351MP retro handheld, code here: https://github.com/Alumniminium/rg351mp
