## Architecture Documentation (.architecture)

The `.architecture` folder contains the authoritative design system for the FTdx101 WebApp. It defines how the application is structured, how subsystems interact, and how contributors must work to avoid architectural drift. Every document in this folder is part of the living architecture and must be kept aligned with the codebase.

### Purpose
- Provide a single source of truth for system design  
- Enforce strict subsystem boundaries  
- Guide contributors and maintainers  
- Prevent architectural drift  
- Support predictable, long‑term evolution  

### What’s Inside
- **Core architecture**: high‑level overview, subsystem boundaries, workflow, diagrams  
- **Enforcement**: drift detection, refactor rules, Copilot alignment tests  
- **Contributor guidance**: onboarding, coding standards, UI guidelines  
- **Supporting docs**: performance notes, release process, evolution rules, roadmap, accessibility, theming, maturity model  
- **Meta**: index and validation checklist  

### How to Use It
- Start with `architecture-overview.md` for the big picture  
- Read `subsystem-boundaries.md` before making any change  
- Use `workflow.md` to understand data flow  
- Run `drift-check.md` when reviewing or refactoring  
- Follow `refactor-session.md` for safe structural changes  
- Use `developer-onboarding.md` for new contributors  

### Maintenance Rules
- Any architectural change must update the relevant documents  
- Subsystem boundaries must never be violated  
- All changes must follow the refactor workflow  
- The architecture must remain consistent with the value pipeline  

This folder is part of the project’s governance layer. Treat it as a protected, high‑integrity space that defines how the entire system works and evolves.
