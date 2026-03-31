# Performance Guidelines

This document defines performance rules for the FTdx101 WebApp to ensure smooth real‑time meter rendering.

## Goals
Maintain 60fps rendering, avoid UI jank, minimise DOM work, and keep the value pipeline efficient.

## Rendering Rules
- Use canvas for all meters.
- Avoid DOM updates during meter refresh.
- Never read layout values inside update loops.
- Precompute static geometry.
- Use requestAnimationFrame only in UI subsystem.

## Gauge Performance
- Gauge creation: gaugeFactory only.
- Gauge updates: update-engine only.
- Avoid per-frame object allocations.
- Reuse buffers and paths where possible.

## Pipeline Efficiency
- WebSocket messages must be lightweight.
- WsUpdatePipeline must avoid branching and heavy logic.
- Calibration must remain pure and fast.
- Orchestrator must not transform data.

## DOM Rules
- DOM access only in MeterPanel.
- Avoid layout thrashing (no offsetWidth/Height reads in loops).
- Batch DOM writes when needed.

## Canvas Rules
- Use a single canvas per meter.
- Avoid clearing full canvas; clear only necessary regions.
- Pre-render static elements when possible.

## Memory Rules
- No global caches unless documented.
- Avoid retaining large objects across frames.
- Release unused gauges cleanly.

## WebSocket Performance
- Avoid sending redundant messages.
- Compress or coalesce updates if backend supports it.
- Validate message size and frequency.

## Serial/Queue Performance
- Maintain correct timing to avoid radio overload.
- Avoid unnecessary CAT polling.
- Use efficient parsing in decoding layer.

## Testing Performance
- Benchmark calibration functions.
- Measure pipeline latency.
- Validate UI frame rate under load.

## When Performance Is At Risk
- Too many DOM operations.
- Gauge updates outside update-engine.
- Calibration doing UI or formatting work.
- WebSocket messages too large or too frequent.

## Summary
Keep rendering in canvas, keep logic pure, keep pipeline lean, keep DOM isolated, and avoid unnecessary work in high-frequency paths.
