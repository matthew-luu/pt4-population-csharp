# Mathematical Model

This document describes the conceptual mathematical model used by the exploit engine.

The exact implementation may evolve, but the core ideas remain the same.

---

## Expected Value Framework

For a given hand and action, expected value is modeled as the weighted sum of branch outcomes.

General form:

```text
EV = Σ P(branch) × EV(branch)
```

This is the foundation of the exploit engine.

---

## Example: Open Decision

Suppose a hand is opened and the population can:

- fold
- call
- 3-bet

Then:

```text
EV_open(hand)
  = P_fold × EV_fold
  + P_call × EV_call
  + P_3bet × EV_3bet
```

Each branch has its own internal assumptions and calculations.

---

## Population Probabilities

Branch probabilities come from the observed population model.

Example:

```text
P_fold = 0.54
P_call = 0.31
P_3bet = 0.15
```

These values are estimated from the population dataset, not from equilibrium assumptions.

---

## Equity and Realization

When a branch continues to a contested pot, raw equity alone is often not enough.

A practical model may use:

```text
realized_equity = raw_equity × realization_factor
```

where the realization factor attempts to account for position, initiative, and postflop playability.

---

## Continue Branches

Some branches require modeling future decisions.

Example:

- population 3-bets after our open
- we may fold, call, or 4-bet
- those sub-branches influence the EV of the original open

This means the exploit engine can model nested strategic consequences rather than only immediate outcomes.

---

## Opportunity-Weighted Inputs

Where priors or estimates are blended, larger sample nodes should generally carry more weight than smaller sample nodes.

This improves stability and reduces overfitting to sparse data.

---

## Practical Goal of the Model

The goal is not to create a perfect theoretical model of poker.

The goal is to create a useful, interpretable, and empirically grounded exploit model that can rank hands and build profitable strategies against a real player pool.