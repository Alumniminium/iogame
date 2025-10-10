# Task Completion Checklist

When completing a coding task in the pixiejsClient project, follow these steps:

## 1. Code Quality Checks

### Linting (REQUIRED)
```bash
npm run lint
```
- Must pass with no errors
- Prettier formatting automatically applied

### Type Checking (REQUIRED)
```bash
npx tsc --noEmit
```
- Must pass with no TypeScript errors
- Strict mode enabled, so all types must be correct

## 2. Build Verification (REQUIRED)
```bash
npm run build
```
- Includes linting automatically
- Verifies production build succeeds
- Generates source maps

## 3. Testing
- **No test framework configured** - User must manually test
- Ask user to run and test when implementation is complete
- Do NOT run dev server (`npm run dev`) - it runs indefinitely

## 4. Git Workflow
- Stage relevant changes only
- Follow repository commit conventions
- Do NOT commit without user request
- Check git status before committing

## Task Completion Flow
1. ✅ Make code changes
2. ✅ Run `npm run lint`
3. ✅ Run `npx tsc --noEmit` 
4. ✅ Run `npm run build` to verify
5. ✅ Ask user to test functionality
6. ✅ Only commit if user requests

## Notes
- Never run indefinite processes (`npm run dev`)
- Always verify with build commands, not runtime
- User is responsible for functional testing
