# Code Style and Conventions

## TypeScript Configuration

### Compiler Options
- **Target**: ES2020
- **Module**: ESNext with bundler resolution
- **Strict mode**: Enabled
- **Unused locals/parameters**: Error
- **No fallthrough cases**: Error
- **Use define for class fields**: true

### Module System
- ES Modules only (`"type": "module"`)
- `.ts` extensions allowed in imports
- JSON module imports enabled
- Isolated modules for faster builds

## ESLint Rules

### Enabled Checks
- TypeScript recommended rules
- Prettier formatting integration
- JavaScript recommended rules

### Disabled Rules (Intentional)
- `@typescript-eslint/no-explicit-any` - OFF (allows `any` type)
- `@typescript-eslint/no-unused-vars` - OFF (handled by TypeScript)
- `no-empty` - OFF (allows empty blocks)
- `no-case-declarations` - OFF (allows declarations in case blocks)

## Formatting
- **Prettier** integrated with ESLint
- Formatting enforced on lint

## Naming Conventions (Observed)
- **Classes**: PascalCase (e.g., `NetworkManager`, `GameScreen`)
- **Interfaces**: PascalCase (e.g., `ComponentQuery`, `SystemDefinition`)
- **Constants**: camelCase (e.g., `engine`, `userSettings`)
- **Files**: PascalCase for classes, camelCase for utilities

## Import Style
- Auto-imports enabled
- Path aliases not configured (uses relative imports)
- Side-effect imports for plugins (e.g., `import "@pixi/sound"`)
