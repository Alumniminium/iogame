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

## Control Flow Patterns

### Early Returns (PREFERRED)
- **Always use early returns** to reduce indentation and scoping
- Avoid nested if statements when possible
- Return/continue/break early on guard conditions

**Preferred:**
```typescript
if (!entity) return;
if (!component) return;

// Main logic here with minimal indentation
processEntity(entity, component);
```

**Avoid:**
```typescript
if (entity) {
  if (component) {
    // Main logic nested inside
    processEntity(entity, component);
  }
}
```

## Naming Conventions (Observed)
- **Classes**: PascalCase (e.g., `NetworkManager`, `GameScreen`)
- **Interfaces**: PascalCase (e.g., `ComponentQuery`, `SystemDefinition`)
- **Constants**: camelCase (e.g., `engine`, `userSettings`)
- **Files**: PascalCase for classes, camelCase for utilities

## Import Style
- Auto-imports enabled
- Path aliases not configured (uses relative imports)
- Side-effect imports for plugins (e.g., `import "@pixi/sound"`)
