# Development

This document describes the internal development structure of `pt4-population-csharp`.

It is intended for contributors and for future maintenance.

---

## Repository Role

This repository is the exploit and modeling layer of the Poker.RangeApprox project.

Its responsibilities begin once a population dataset already exists.

---

## Conceptual Module Breakdown

Typical areas of responsibility include:

- application entry points
- input parsing and loading
- population node modeling
- ranking and range approximation
- exploit EV evaluation
- output formatting
- GUI interaction

---

## Common Layer Responsibilities

### Core layer

Contains domain concepts and strategy logic such as:

- population nodes
- range approximation
- exploit calculations
- EV result models

### Infrastructure layer

Contains supporting logic such as:

- CSV parsing
- PostgreSQL access
- file loading
- output writers or formatters

### App layer

Provides executable entry points and orchestration.

### GUI layer

Provides a local desktop interface for running the engine and reviewing results.

---

## Development Principles

### Do not hide assumptions

If a model choice affects strategy output, it should be visible in code and documentation.

### Keep the pipeline traceable

It should be possible to explain how an output was produced from the input data.

### Prefer explicit domain models

Named domain types are easier to reason about than loosely structured dictionaries flowing everywhere.

### Preserve explainability

Branch-EV visibility is a feature, not just a debugging aid.

---

## Extending the Engine

When adding a new exploit scenario, the work usually involves:

1. defining the relevant population inputs
2. deciding how to approximate the needed ranges
3. defining the EV branches
4. implementing result models
5. formatting the output clearly
6. validating against sample data

---

## Testing Guidance

Useful test categories include:

- parser tests
- ranking-selection tests
- range-approximation tests
- EV-calculation tests
- output-formatting tests

Where possible, prefer focused deterministic tests over large opaque integration-only tests.