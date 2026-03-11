# Configuration

This document describes the main categories of settings that affect how the exploit engine runs.

The exact configuration surface may evolve over time.

---

## Data Source Configuration

The engine can be configured to read from:

- CSV
- PostgreSQL

Typical concerns include:

- file paths
- connection strings
- dataset selection
- environment-specific overrides

---

## Game Parameters

Important modeling parameters may include:

- stack depth
- rake percentage
- rake cap
- blind size assumptions

These values influence EV calculations and therefore strategy outputs.

---

## Ranking Configuration

Range approximation depends on ranking profiles.

Configuration may control:

- which profile is used for a node
- where rankings are loaded from
- fallback profile behavior
- profile naming and selection

---

## Output Configuration

Useful output controls may include:

- console formatting
- export path
- level of branch detail
- whether to include per-hand EV decomposition

---

## Development Guidance

When changing configuration, prefer:

- explicit values
- documented defaults
- reproducible settings across runs

Undocumented “magic” settings make strategy comparisons harder to trust.