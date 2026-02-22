---
title: Status and Roadmap
nav_order: 2
---

# NativeData Status and Roadmap

This document captures what is implemented today, what is intentionally out of scope, and a proposed roadmap for getting NativeData from MVP foundation to a production-ready AOT/trimming-compliant ORM.

Release execution guidance: see [release-checklist.md](release-checklist.md).

## Vision

NativeData aims to provide a **nativeAOT/trimming-friendly ORM** for .NET 10 by favoring:

- compile-time metadata over runtime reflection
- explicit provider wiring over dynamic provider loading
- predictable SQL generation over broad dynamic conventions

## What Exists Today

## Solution and Projects

- `NativeData.Abstractions`
  - Core contracts and primitives:
    - `IRepository<T>`
    - `ICommandExecutor`
    - `IEntityMap<T>`
    - `ISqlDialect`
    - `IDbConnectionFactory`
    - `SqlParameterValue`
    - `NativeDataEntityAttribute`

- `NativeData.Core`
  - Runtime implementations:
    - `SqlRepository<T>`
    - `DbCommandExecutor`
    - `DefaultSqlDialect`

- `NativeData.Sqlite`
  - First provider package:
    - `SqliteConnectionFactory`
    - `SqliteSqlDialect`
  - Depends on `Microsoft.Data.Sqlite`

- `NativeData.Generators`
  - Source generator scaffold (`NativeDataEntityGenerator`)
  - Currently discovers `[NativeDataEntity]` types and emits a simple registry

- `NativeData.Analyzers`
  - Analyzer scaffold (`TrimSafetyAnalyzer`)
  - Currently flags `Type.GetType(...)` usage (`ND0001`) as trim/AOT-risky

- `NativeData.Tests`
  - Unit tests for repository SQL generation
  - SQLite round-trip integration test

- `NativeData.AotSmoke` (sample)
  - Creates a SQLite DB
  - Creates table, inserts entity, loads entity by id
  - Used as publish smoke target for `PublishAot + PublishTrimmed`

## Current Functional Surface

- CRUD-style repository operations
  - `GetByIdAsync`
  - `QueryAsync(whereClause, parameters)`
  - `InsertAsync`
  - `UpdateAsync`
  - `DeleteByIdAsync`

- Dialect abstraction for:
  - identifier quoting
  - parameter name normalization and rendering

- Provider-agnostic execution via ADO.NET abstractions

## Quality/Validation in Place

- Solution builds with `-warnaserror`
- Unit and integration tests pass
- AOT trimmed publish smoke check passes for sample

## Known Gaps (By Design)

The following are intentionally not implemented in current MVP:

- migrations
- change tracking / identity map
- LINQ translation/provider
- generated entity materializers and map emitters (generator is scaffold only)
- analyzer pack coverage beyond a starter rule
- provider packages beyond SQLite

## Architecture Notes

NativeData is currently split in a way that supports long-term maintainability:

- contracts (`Abstractions`) are isolated from runtime implementation (`Core`)
- provider-specific behavior lives in provider packages (currently SQLite)
- compile-time tooling is separated (`Generators`, `Analyzers`)
- sample and tests validate real behavior and AOT publish workflow

This layout is suitable for adding providers (PostgreSQL, SQL Server) and for evolving generator/analyzer capabilities without destabilizing the runtime API.

## Proposed Roadmap

## Milestone v0.2.0 — Foundation Hardening

Focus:

- stabilize current APIs and tighten quality checks

Target deliverables:

- public API docs for `Abstractions` and `Core`
- expanded tests for SQL edge cases (nulls, parameter prefixes, update behavior)
- package metadata baseline (versioning, descriptions, release notes template)

Acceptance criteria:

- all public APIs compile with XML docs enabled and no documentation warnings
- test suite includes edge-case coverage for CRUD SQL generation
- `dotnet build -warnaserror`, `dotnet test`, and AOT smoke publish pass in CI

Progress (2026-02-21):

- completed: XML documentation enabled for `NativeData.Abstractions` and `NativeData.Core`
- completed: public API XML docs added across `Abstractions` and `Core`
- completed: SQL edge-case tests expanded (parameter prefixes, key-only update guard, null parameter values, whitespace `whereClause` behavior)
- completed: package metadata baseline added (versioning, description, tags, repository URL, release notes template)
- completed: CI workflow added for v0.2 gates (`.github/workflows/ci.yml`) covering build, test, and AOT smoke publish
- completed: local validation passes (`dotnet build NativeData.slnx`, `dotnet test`)
- remaining: none identified for v0.2 foundation hardening scope

## Milestone v0.3.0 — Generated Mapping (First Usable)

Focus:

- reduce manual mapping by shipping a practical generator path

Target deliverables:

- generator emits `IEntityMap<T>` for annotated entities
- generated materialization and parameter binding for common entity shapes
- diagnostics for unsupported constructors/properties

Acceptance criteria:

- at least one sample entity runs end-to-end using generated map (no handwritten map)
- generator output is deterministic across repeated builds
- benchmark shows generated mapping is not slower than current manual baseline in hot paths

Progress (2026-02-22):

- completed: source generator emits `IEntityMap<T>` and public generated map factory (`NativeDataEntityMaps.Create<T>()`)
- completed: AOT smoke sample uses generated mapping end-to-end (no handwritten map)
- completed: deterministic generation test added (`Generator_Output_IsDeterministic_AcrossRuns`)
- completed: benchmark comparison added and executed (`dotnet run -c Release --project benchmarks/NativeData.Benchmarks/NativeData.Benchmarks.csproj`), with generated mapping equal/faster than manual across measured insert/update/materialize paths

## Milestone v0.4.0 — Analyzer Pack Expansion

Focus:

- enforce AOT/trimming-safe usage with actionable diagnostics

Target deliverables:

- expanded rules for reflection-heavy and trim-risky patterns
- analyzer release tracking metadata and packaged analyzer quality polish
- remediation docs for each analyzer rule

Acceptance criteria:

- analyzer package emits documented diagnostics with stable IDs/messages
- each rule has at least one positive and one negative test case
- consumers can enable analyzers without introducing false-positive noise in sample app

## Milestone v0.5.0 — Second Provider Validation

Focus:

- prove provider abstraction beyond SQLite

Target deliverables:

- `NativeData.Postgres` (Npgsql) provider package
- dialect-specific SQL/parameter tests shared with SQLite where possible
- provider compatibility matrix in docs

Acceptance criteria:

- shared repository behavioral tests pass for SQLite and Postgres
- sample app (or sample variant) executes CRUD against Postgres
- AOT trimmed publish remains successful with provider package referenced

## Milestone v1.0.0 — Production Baseline

Focus:

- lock stable API and operational confidence for first general release

Target deliverables:

- API freeze for core contracts
- finalized docs (getting started, provider setup, diagnostics, limitations)
- CI gates for build/test/AOT smoke + package validation

Acceptance criteria:

- semver-stable public API approved and tagged
- generated mapping + analyzer pack + at least two providers are release-quality
- release checklist completed (tests, performance sanity, docs, package metadata)

## Post-v1 Backlog (Optional)

- optional lightweight unit-of-work helpers
- repository convenience extensions
- compile-time-safe conventions package

## Non-Goals (Current Direction)

Unless strategy changes, NativeData should avoid in core:

- runtime expression compilation/query providers with broad dynamic behavior
- runtime assembly scanning/discovery
- proxy-based tracking features that compromise AOT/trimming guarantees

## Definition of “v1 Ready” (Suggested)

NativeData can be considered v1-ready when:

- public API is documented and stable
- generated mapping is production-usable
- analyzer coverage guards the main AOT/trim pitfalls
- at least two providers pass shared compatibility tests
- CI includes build, tests, and AOT publish smoke checks
