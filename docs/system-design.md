# System Design

Poker.RangeApprox is a multi-stage system that turns historical PokerTracker 4 data into exploitative preflop strategy recommendations.

This document describes the full system across both repositories.

---

## Full Pipeline

```text
PokerTracker4 Database
        ↓
pt4-population-sql
  - extract preflop opportunities
  - count actions
  - compute frequencies
        ↓
Population Dataset
        ↓
pt4-population-csharp
  - approximate ranges
  - evaluate EV
  - construct exploit strategies
        ↓
Exploit Strategy Output
```

---

## Repository Responsibilities

### `pt4-population-sql`

Responsible for:

- aggregating PT4 hand data
- counting opportunities
- computing action frequencies
- exposing the population dataset

### `pt4-population-csharp`

Responsible for:

- loading the dataset
- approximating ranges
- evaluating hand-level EV
- generating exploit strategies
- presenting results

---

## Why the System Is Split Into Two Repositories

The project separates data engineering from modeling.

Benefits include:

- clearer responsibility boundaries
- easier experimentation
- independent iteration on SQL and C# layers
- easier debugging when validating population statistics

---

## Data Flow

### Stage 1: Historical observation

PT4 stores hand-level records of what players actually did.

### Stage 2: Population aggregation

The SQL repository groups those observations into node-based frequencies and counts.

### Stage 3: Range reconstruction

The C# repository converts those frequencies into approximate ranges.

### Stage 4: Exploit analysis

The exploit engine evaluates all candidate hands against the reconstructed population model.

### Stage 5: Strategy construction

Hands are ranked by EV and selected into exploit ranges.

---

## EV Tree Concept

A candidate action is modeled as a branch tree.

Example conceptual open decision:

```text
Open Hand
   │
   ├── Population folds
   │       immediate profit
   │
   ├── Population calls
   │       realized equity branch
   │
   └── Population 3-bets
           continue / fold / 4-bet logic
```

This is what allows the engine to explain not just *which* hands are profitable, but *why* they are profitable.

---

## Important Design Choices

### Use frequencies from real populations

The system is designed for practical exploitation, not equilibrium solving.

### Reconstruct ranges from rankings

PT4 gives frequencies, not explicit hand distributions.

### Preserve sample-size information

Opportunity counts matter for model confidence and weighting.

### Keep the pipeline modular

The SQL extraction layer and the exploit layer should remain independently understandable.