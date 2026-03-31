# FTdx101 WebApp — System Workflow

This document describes the end‑to‑end workflow of the FTdx101 WebApp, covering both frontend and backend flows. It explains how data moves through the system and how each subsystem participates in the process.

---

# 1. Frontend Workflow (Meters)

This is the core real‑time update loop that drives the UI meters.

## Step 1 — WebSocket Receives Raw Data
- Backend sends CAT updates via WebSocket.
- WsConnection receives raw payloads.
- No parsing, calibration, or UI logic occurs here.

## Step 2 — WsUpdatePipeline Routes Messages
- Parses message type.
- Normalises raw values.
- Routes values to the calibration engine.
- Contains no DOM or UI logic.

## Step 3 — Calibration Engine Converts Values
- Pure functions apply scaling and empirical corrections.
- Produces calibrated meter values.
- No DOM, no WebSocket, no UI logic.

## Step 4 — FTdx101Meters Orchestrates Flow
- Receives calibrated values.
- Passes them to MeterPanel.update().
- Contains no calibration, formatting, or DOM logic.

## Step 5 — MeterPanel Updates UI
- Delegates gauge creation to gaugeFactory.
- Delegates gauge updates to update-engine.
- Delegates text formatting to meter-formatters.
- Owns all DOM access.

## Step 6 — Gauges Render to Canvas
- Canvas-gauge library draws final visuals.
- Rendering is isolated to UI subsystem.

---

# 2. Backend Workflow (CAT)

This describes how the backend communicates with the radio and prepares data for the frontend.

## Step 1 — Serial Layer Communicates with Radio
- Sends CAT commands.
- Receives raw CAT responses.
- Handles timing and port state.

## Step 2 — Queue Layer Manages Command Flow
- Ensures correct sequencing.
- Prevents command collisions.
- Maintains timing guarantees.

## Step 3 — Decoding Layer Interprets Responses
- Parses CAT strings.
- Extracts raw meter values.
- Normalises radio state.

## Step 4 — Backend Pushes Updates via WebSocket
- Sends raw values to frontend.
- Frontend pipeline begins.

---

# 3. UI Workflow

## Layout System
- CSS grid/flexbox.
- Responsive behaviour.
- No inline layout logic except canvas size.

## UI State
- DOM access only in UI subsystem.
- Canvas rendering.
- Overlay management.
- User interaction handling.

---

# 4. Error Handling Workflow

## WebSocket Errors
- Reconnect logic.
- Connection state reporting.

## Serial Errors
- Timeout handling.
- Port reset logic.

## Calibration Errors
- Fallback values.
- Safe defaults.

## UI Errors
- Graceful degradation.
- Non-blocking rendering.

---

# 5. Enforcement

This workflow is enforced by:
- `.copilot/rules.md`
- `drift-check.md`
- Folder structure
- Orchestrator design
- Refactor workflow

All code must follow this workflow exactly.
