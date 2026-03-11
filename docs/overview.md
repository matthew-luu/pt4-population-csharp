# Overview

`pt4-population-csharp` generates exploitative preflop strategies from population data.

Rather than solving for equilibrium, the engine attempts to answer a practical question:

> Given how the population actually behaves, which strategy maximizes expected value?

---

## The Core Idea

If the population is known to:

- over-fold
- under-3-bet
- over-call
- continue too tightly or too loosely

then those tendencies create profitable deviations from equilibrium play.

This repository models those tendencies and turns them into strategy recommendations.

---

## Inputs

The engine consumes a population dataset containing statistics such as:

- RFI frequencies
- defense frequencies vs opens
- response frequencies vs 3-bets
- opportunity counts

These inputs come from the companion SQL repository.

---

## Main Processing Stages

```text
Population frequencies
        ↓
Range approximation
        ↓
Population range model
        ↓
EV evaluation across 169 hands
        ↓
Exploit range construction
```

---

## Outputs

The engine produces outputs such as:

- exploit opening ranges
- exploit 3-bet ranges
- hand rankings by EV
- branch-level EV breakdowns

---

## Why Range Approximation Is Needed

The population dataset contains frequencies, not explicit hand ranges.

For example:

```text
BTN open frequency = 48%
```

That tells us how often the population opens, but not which 48% of hands they open.

This repository approximates those ranges using ranking profiles and model assumptions.

---

## Why This Repository Exists

The goal is not just to measure population frequencies.  
It is to turn those frequencies into **actionable exploit strategies**.