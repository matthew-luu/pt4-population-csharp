# Usage

This document explains how to run `pt4-population-csharp`.

Because setups differ, the exact commands and options may vary slightly by environment and current application wiring.

---

## Prerequisites

You generally need:

- .NET SDK installed
- the companion population dataset
- either CSV exports or a PostgreSQL connection
- any required local ranking or configuration files

---

## Typical Workflow

### 1. Build or obtain the population dataset

First, generate the population statistics using the companion repository:

**`pt4-population-sql`**

This produces the frequencies and counts the engine consumes.

---

### 2. Choose input mode

Supported input modes may include:

- CSV input
- PostgreSQL input

Use whichever path matches your local setup.

---

### 3. Run the console application

Conceptually:

```text
dotnet run --project src/Poker.RangeApprox.App
```

Use the application’s configured options to point it at the required dataset and settings.

---

### 4. Run the GUI

Conceptually:

```text
dotnet run --project src/Poker.RangeApprox.WinForms
```

The GUI is useful for local experimentation, parameter adjustments, and reviewing outputs more conveniently than via terminal-only flows.

---

### 5. Review the results

Typical outputs may include:

- exploit ranges
- hand rankings
- total EV by hand
- branch EV decomposition
- summaries by node

---

## Validation Checklist

Before trusting the results, verify:

- the input dataset is from the intended limit or pool
- opportunities are sufficiently large
- configuration values match the environment being modeled
- the chosen ranking assumptions are appropriate for the node
- outputs are directionally sensible

---

## Common Development Pattern

A common workflow is:

1. refresh the SQL population dataset
2. run the C# engine
3. inspect outputs
4. refine assumptions
5. rerun and compare

This loop is central to improving exploit quality.