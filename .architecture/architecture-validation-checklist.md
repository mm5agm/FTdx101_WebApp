# Architecture Validation Checklist
Checklist to confirm the .architecture suite is complete and aligned.
## Core Files
[ ] architecture-overview.md  
[ ] subsystem-boundaries.md  
[ ] workflow.md  
[ ] diagrams-overview.md  
## Enforcement
[ ] drift-check.md  
[ ] refactor-session.md  
[ ] copilot-compliance-test.md  
## Contributor Docs
[ ] developer-onboarding.md  
## Optional/Supporting
[ ] performance-guidelines.md  
[ ] ui-style-guide.md  
[ ] coding-standards.md  
[ ] release-process.md  
[ ] subsystem-evolution.md  
[ ] architecture-roadmap.md  
[ ] ui-accessibility.md  
[ ] ui-theming.md  
[ ] architecture-maturity-model.md  
## Structural Checks
[ ] All files exist in .architecture/  
[ ] No file contradicts subsystem-boundaries.md  
[ ] No file introduces cross-subsystem logic  
[ ] All documents reference the same pipeline model  
[ ] All documents use consistent terminology  
## Completeness Checks
[ ] INDEX.md lists all files  
[ ] No orphan documents  
[ ] No TODO placeholders  
[ ] Diagrams folder exists (even if empty)  
## Maintenance Rules
[ ] Any architecture change updates relevant docs  
[ ] Refactor-session.md used for structural changes  
[ ] Drift-check.md run after major edits  
## Summary
If all boxes are checked, the architecture suite is complete, consistent, and aligned.
