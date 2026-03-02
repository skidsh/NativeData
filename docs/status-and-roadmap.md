---
title: Status and Roadmap
nav_order: 2
---

# NativeData Status and Roadmap

The live backlog, milestone progress, and prioritized work items are tracked in the **[NativeData GitHub Project](https://github.com/users/kylek-dev/projects/2)**.

Release execution guidance: see [release-checklist.md](release-checklist.md).

## Vision

NativeData aims to provide a **nativeAOT/trimming-friendly ORM** for .NET 10 by favoring:

- compile-time metadata over runtime reflection
- explicit provider wiring over dynamic provider loading
- predictable SQL generation over broad dynamic conventions

## What's Implemented

| Package | Status |
|---|---|
| `NativeData.Abstractions` | ✅ Stable — contracts, `IRepository<T>`, `ISqlDialect`, `IEntityMap<T>` |
| `NativeData.Core` | ✅ Stable — `SqlRepository<T>`, `DbCommandExecutor`, CRUD |
| `NativeData.Sqlite` | ✅ Stable — SQLite provider (Microsoft.Data.Sqlite) |
| `NativeData.Postgres` | ✅ Stable — PostgreSQL provider (Npgsql) |
| `NativeData.Generators` | ✅ Stable — Roslyn source generator, emits `IEntityMap<T>` at compile time |
| `NativeData.Analyzers` | ✅ Stable — 5 diagnostic rules (ND0001–ND1002) |

See [providers.md](providers.md) for the provider compatibility matrix.

## Roadmap

The full milestone backlog, priorities, and work item status are tracked in the **[NativeData GitHub Project](https://github.com/users/kylek-dev/projects/2)**.

| Milestone | Focus | Status |
|---|---|---|
| v0.2.0 | Foundation hardening — XML docs, CI, edge-case tests | ✅ Done |
| v0.3.0 | Source-generated `IEntityMap<T>` end-to-end | ✅ Done |
| v0.4.0 | Analyzer pack expansion (ND0001–ND1002) | ✅ Done |
| v0.5.0 | Second provider — PostgreSQL (Npgsql) | ✅ Done |
| v0.6.0 | LINQ-style fluent query builder (`NativeDataQuery<T>`) | ✅ Done |
| v0.7.0 | `NativeDataContext` + DI integration (`AddNativeData<T>`) | 🔲 Planned |
| v1.0.0 | API freeze, full docs, production baseline | 🔲 Planned |

## Architecture

- `Abstractions` — contracts isolated from runtime implementation
- `Core` — ADO.NET implementation, provider-agnostic
- `Sqlite` / `Postgres` — provider-specific behavior in dedicated packages
- `Generators` / `Analyzers` — compile-time tooling, no runtime overhead
- `samples/` — AOT publish smoke tests and provider samples

## Non-Goals

NativeData intentionally avoids in core:

- runtime expression compilation (LINQ translation is source-generated or compile-time-bounded)
- runtime assembly scanning or dynamic provider loading
- proxy-based change tracking (incompatible with AOT/trimming)
- migrations
