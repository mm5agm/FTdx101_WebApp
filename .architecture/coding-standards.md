# Coding Standards

This document defines coding rules to ensure consistency, clarity, and architectural alignment.

## Principles
- Code must be predictable, readable, and modular.
- Subsystem boundaries must be respected.
- Avoid cleverness; prefer clarity.
- No global state.

## Language Rules
- ES modules only.
- Use const by default; let when needed; never var.
- Arrow functions preferred for small utilities.
- Classes used only where architectural roles require them.

## Naming
- Files: kebab-case.
- Variables: camelCase.
- Classes: PascalCase.
- Constants: UPPER_SNAKE_CASE.
- Functions: verbs; pure where possible.

## Structure
- One responsibility per file.
- Keep functions small and focused.
- Avoid deep nesting.
- Prefer early returns over complex branching.

## Imports
- No circular imports.
- Import only what you use.
- Keep import paths relative and minimal.

## Error Handling
- Fail fast in logic layers.
- Graceful handling in UI and WebSocket layers.
- No silent failures.

## Comments
- Use comments for intent, not restating code.
- Document empirical behaviour clearly.
- Keep comments short and relevant.

## Formatting
- Consistent indentation (2 spaces).
- No trailing spaces.
- Keep lines reasonably short.
- Use template literals for multi-part strings.

## Performance
- Avoid unnecessary allocations in hot paths.
- Reuse objects where safe.
- Keep UI updates minimal.
- Never block the main thread.

## Testing
- Write tests for pure logic.
- Avoid DOM-heavy tests.
- Mock external systems lightly.

## Forbidden
- Global variables.
- Inline DOM manipulation outside UI subsystem.
- Gauge creation outside gaugeFactory.
- Gauge updates outside update-engine.
- Formatting outside meter-formatters.
- Calibration outside calibration-engine.

## Summary
Write clear, modular, predictable code that respects subsystem boundaries and supports long-term maintainability.
