# Architecture

`pt4-population-csharp` is organized as a modeling pipeline that converts aggregate population statistics into exploit strategy outputs.

---

## High-Level Architecture

```text
Population dataset
        ↓
Parsing / loading
        ↓
Population node model
        ↓
Range approximation engine
        ↓
Exploit EV engine
        ↓
Formatting / reporting
        ↓
Console or GUI output
```

---

## Main Components

### 1. Input Layer

The input layer reads the population dataset from either:

- CSV
- PostgreSQL

Its job is to load frequencies, counts, and metadata into application models.

---

### 2. Population Model Layer

This layer represents the observed behavior of the pool in structured form.

Examples include:

- node identifiers
- node-family frequencies
- opportunity counts
- grouped response distributions

---

### 3. Range Approximation Layer

Because the dataset does not contain exact ranges, this layer reconstructs approximate population ranges using:

- ranking profiles
- target frequencies
- super-range construction where needed

This is the bridge from aggregate frequencies to hand-level modeling.

---

### 4. Exploit Engine

The exploit engine evaluates all 169 hands in a spot and computes EV against the population model.

It can decompose EV into branches such as:

- fold branch EV
- call branch EV
- 3-bet branch EV
- 4-bet branch EV

depending on the node being modeled.

---

### 5. Output Layer

This layer formats the result into:

- ranked hand outputs
- exploit ranges
- branch-EV summaries
- GUI displays

---

## Design Principles

### Separation of concerns

Parsing, range approximation, EV evaluation, and presentation should remain separate.

### Explicit modeling

Population assumptions should be visible in code and documentation.

### Extensibility

New node families and exploit scenarios should be addable without breaking the existing architecture.

### Explainability

A result should be traceable back to the assumptions and branches that produced it.