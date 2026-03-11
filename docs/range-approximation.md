# Range Approximation

The exploit engine needs explicit or semi-explicit hand ranges, but the population dataset only contains aggregate frequencies.

For example:

```text
BTN RFI frequency = 47.8%
```

This tells us how often the population opens, but not which exact hands make up that 47.8%.

Range approximation is the process of converting those frequencies into approximate hand sets.

---

## Why Approximation Is Necessary

The source population data describes **how often** actions happen.

The exploit engine needs to estimate **which hands** are associated with those actions.

Without range approximation, EV cannot be evaluated hand by hand.

---

## Core Approach

A common approximation workflow is:

1. choose a ranking profile for the node
2. sort all 169 starting hands by that ranking
3. take the top portion needed to match the target frequency

Example:

```text
target open frequency = 25%

approximate open range = top 25% of hands under the chosen ranking profile
```

---

## Ranking Profiles

Different strategic nodes may use different ranking profiles.

Examples might include:

- heads-up all-in equity
- PokerStove-style equity ordering
- Sklansky-Chubukov style ordering
- domain-specific heuristic profiles

The chosen ranking should match the strategic meaning of the node as closely as possible.

---

## Super-Ranges

Some behaviors require combining actions into a larger “continue” concept.

Example:

```text
continue vs 3-bet = call range + 4-bet range
```

This is useful when modeling whether an opening hand survives future branches.

---

## Opportunity-Weighted Thinking

If multiple estimates or priors are combined, opportunity counts can be used to weight them.

This helps avoid overreacting to low-sample statistics.

---

## Limits of the Approximation

This method does not claim to recover the true exact population range.

It produces a practical approximation suitable for exploit modeling.

The goal is useful decision support, not perfect inverse reconstruction.

---

## Why This Step Is So Important

Range approximation is the bridge between:

- aggregate statistics
- hand-level EV analysis

It is one of the key ideas that makes the entire system possible.