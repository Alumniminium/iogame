# Suggested Development Commands

## Primary Development Commands

### Build and Development
```bash
# Development server (DO NOT RUN - runs indefinitely)
npm run dev          # Vite dev server on port 8080

# Production build
npm run build        # Runs lint + vite build

# Lint only
npm run lint         # ESLint check
```

### Common Workflows
```bash
# Type checking (important before committing)
npx tsc --noEmit

# Build for production
npm run build        # Includes linting
```

## Important Notes
- **DO NOT run `npm run dev`** - The dev server runs indefinitely and only the user can test
- **DO NOT run `npm start`** - Alias for `npm run dev`
- Always run `npm run lint` or `npm run build` to verify changes
- Use `npx tsc --noEmit` for type checking without building

## System Commands (Linux)
Since the system is Linux, standard Unix commands apply:
- `ls` - List files
- `cd` - Change directory
- `grep` - Search text
- `find` - Find files
- `git` - Version control

## Vite Configuration
- Dev server port: 8080
- Auto-open browser: disabled
- Source maps: enabled in production builds
- AssetPack plugin: integrated for asset processing
