# Testing Strategy

This file defines how testing reinforces the architecture.

## Philosophy
Tests must reinforce subsystem boundaries, pure logic, minimal DOM, pipeline integrity, and drift detection.

## Calibration Engine
Test: pure functions, input→output, edge cases.  
Do NOT test: DOM, UI, WebSocket, gauges.

## Decoding
Test: CAT parsing, errors, quirks.  
Do NOT test: UI, DOM, gauges.

## Serial/Queue
Test: ordering, timing, retries.  
Do NOT test: UI, DOM, calibration.

## WebSocket
Test: routing, errors, pipeline entry.  
Do NOT test: UI, DOM, calibration, gauges.

## Meter Subsystem
Test: MeterPanel behaviour, gaugeFactory creation, update-engine updates, meter-formatters formatting.  
Avoid: full DOM snapshots, canvas internals.

## Orchestrator
Test: wiring only.  
Do NOT test: logic, DOM, calibration, gauges.

## Do NOT Test
Avoid: end-to-end DOM, full canvas, cross-subsystem tests, browser simulation, real serial hardware, gauge internals.

## Pipeline Validation
Ensure:  
WebSocket → WsUpdatePipeline → calibration-engine → FTdx101Meters → MeterPanel.update() → gaugeFactory/update-engine → canvas  
Check: correct inputs/outputs, no bypasses, no misplaced logic.

## Drift Detection
Tests should catch: subsystem leakage, formatting drift, calibration contamination, DOM misuse, gauge logic outside correct modules, orchestrator logic creep, folder drift.

## Test Types
Unit: calibration, decoding, formatting, routing.  
Integration: WebSocket→pipeline, orchestrator→MeterPanel.  
UI: minimal DOM/canvas init.  
Mock: WebSocket, Serial, DOM (light), gauge library.  
Do NOT mock: calibration, formatting.

## Folder Structure
tests/calibration  
tests/decoding  
tests/serial  
tests/websocket  
tests/orchestrator  
tests/meters  
tests/ui

## Tooling
Use: Jest, jsdom, lightweight mocks.  
Avoid: heavy DOM frameworks, browser automation, canvas snapshots.

## Coverage
High: calibration, decoding, formatting, routing.  
Moderate: UI, orchestrator.  
Low: canvas internals.

## Maintenance
Update tests, mocks, folders, and docs whenever architecture changes.
