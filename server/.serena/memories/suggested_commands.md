# Suggested Commands for Server Development

## Build Commands

### Build the server
```bash
dotnet build server/server.csproj
```

### Build from project root (entire solution)
```bash
cd /home/trbl/code/Personal/GameDev/iogame
dotnet build iogame.sln
```

### Build from server directory
```bash
cd /home/trbl/code/Personal/GameDev/iogame/server
dotnet build
```

## Run Commands

### Run the server in development mode
```bash
cd /home/trbl/code/Personal/GameDev/iogame/server
dotnet run
```

**NOTE**: Do NOT run the server automatically - the process doesn't exit and only the user can test. Ask the user to run and test when needed.

### Debug the server (VSCode)
Use the "SERVER" launch configuration in `.vscode/launch.json`

## Verification Commands

### Check for compilation errors
```bash
dotnet build server/server.csproj
```

### Restore dependencies
```bash
dotnet restore server/server.csproj
```

## System Utilities (Linux)
- **List files**: `ls` or use the list_dir tool
- **Find files**: `find` or use the find_file tool
- **Search content**: `grep` or use the search_for_pattern tool
- **Git operations**: `git status`, `git diff`, `git log`, etc.

## Important Notes
- No linting or formatting tools configured for C# in this project
- Testing framework not configured
- Always build after making changes to verify compilation
- The server runs at 60 TPS (ticks per second)