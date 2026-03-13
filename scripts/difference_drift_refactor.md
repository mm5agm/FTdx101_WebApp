Drift Check vs Refactor Session — Side‑by‑Side Guide
This comparison helps contributors instantly understand the difference between the two workflows and when to use each one.
Summary:
# Drift Check vs Refactor Session — Micro Summary

• Drift Check = Detect problems. Refactor Session = Fix problems.  
• Drift Check → diagnostic report. Refactor Session → code changes.  
• Both load global rules + architecture maps.  
• Drift Check finds naming drift, boundary violations, duplication, folder drift, empirical mismatches.  
• Refactor Session repairs naming, boundaries, helpers, comments, structure, empirical behaviour.  
• Drift Check output becomes input to Refactor Session.  
• Refactor Session output becomes new ground truth.  
• Architecture maps must stay updated after major changes.  
• Run Drift Check when unsure. Run Refactor Session when ready to fix.  
• Together they form the closed, self‑correcting architecture loop.

Purpose
Drift Check — Detects problems. Produces a diagnostic report.

Refactor Session — Fixes problems. Applies structured improvements.

What You Paste Into Copilot
Drift Check — drift-check.md + list of files to analyse

Refactor Session — refactor-session.md + list of files to refactor

What Copilot Loads
Both workflows load:

Global rules from .github/copilot-instructions.md

Architecture maps from scripts/

subsystem boundaries

helper map

command flow

radio state

empirical rules

architecture overview

The difference is in what Copilot does with that information.

What Copilot Does
Drift Check
Scans files for:

naming drift

subsystem violations

duplicated logic

folder/namespace drift

outdated comments

empirical mismatches

Groups findings by subsystem

Produces a structured drift report

Does not modify code

Refactor Session
Uses drift report + architecture maps

Generates coherent refactor chunks

Fixes:

naming

boundaries

helpers

comments

structure

empirical behaviour alignment

Produces code changes

Continues until architecture is clean

Output
Drift Check Output
Summary of architectural health

Findings by subsystem

Naming drift list

Duplicated logic list

Folder/namespace drift list

Recommended refactor chunks

Refactor Session Output
A sequence of refactor chunks

Each chunk includes:

summary of drivers

subsystem‑grouped rationales

coherent improvements

code changes

When to Use Each
Use a Drift Check When:
You want to understand the current state

You suspect drift but aren’t sure where

You want a diagnostic report

You’re preparing for a refactor session

Use a Refactor Session When:
You already know what needs fixing

You have a drift report

You want Copilot to apply structured improvements

You want to realign the architecture

What Happens After
After a Drift Check
Review the report

Run a refactor session

After a Refactor Session
Architecture is clean

Maps may need updating

Next drift check will confirm alignment

Summary Table
Aspect	Drift Check	Refactor Session
Goal	Detect drift	Fix drift
Input	drift-check.md + file list	refactor-session.md + file list
Output	Diagnostic report	Refactor chunks + code changes
Modifies Code	No	Yes
Uses Architecture Maps	Yes	Yes
Uses Global Rules	Yes	Yes
When to Run	Before refactoring	After drift check
End Result	List of issues	Clean, aligned architecture

