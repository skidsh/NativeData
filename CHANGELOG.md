# Changelog

All notable changes to NativeData are documented here.

Format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
Versions follow [Semantic Versioning](https://semver.org/).

---

## [Unreleased] — v1.0.0

### Added
- `docs/getting-started.md` — comprehensive guide covering installation, entity definition, DI and manual setup, CRUD, query builder, and analyzer diagnostics

### Changed
- `RepositoryUrl` and `PackageProjectUrl` in `Directory.Build.props` corrected to `kylek-dev/NativeData`
- `docs/status-and-roadmap.md` updated: v0.7.0 marked Done, v1.0.0 marked In Progress; package table now includes `NativeData.Extensions.DependencyInjection` and the correct analyzer rule count (6)
- `docs/index.md` updated: stale URLs corrected, capability list updated to reflect v1.0.0 feature set

### Removed
- `TrimSafetyAnalyzer.DiagnosticId` redundant public constant (was an alias for `TypeGetTypeDiagnosticId`); use `TypeGetTypeDiagnosticId` directly

---

## [0.7.1] — 2026-03-22

### Added
- `README.md` for `NativeData.Extensions.DependencyInjection` package documenting `AddNativeData<TContext>()`, service lifetimes, AOT notes, and usage examples

---

## [0.7.0] — 2026-03-22

### Added
- **`NativeData.Extensions.DependencyInjection`** — new package providing `AddNativeData<TContext>()` for `Microsoft.Extensions.DependencyInjection`
  - `NativeDataOptions` builder with `UseSqlite()` and `UsePostgres()` extension points
  - Registers `TContext` as Scoped, `IDbConnectionFactory` and `ISqlDialect` as Singletons
  - `[DynamicallyAccessedMembers(PublicConstructors)]` on `TContext` for AOT safety
- **`NativeDataContext`** abstract base class in `NativeData.Core`
  - `RegisterMap<T>(IEntityMap<T>)` for compile-time map registration
  - `Repository<T>()` with per-scope caching; no open connections held between calls
  - Implements `IAsyncDisposable`
- **`UseSqlite(NativeDataOptions, string)`** extension method in `NativeData.Sqlite`
- **`UsePostgres(NativeDataOptions, string)`** extension method in `NativeData.Postgres`
- **`ND0004`** analyzer rule — warns on `Expression.Compile()` / `LambdaExpression.Compile()` usage
- Shared LINQ query behavior tests covering both SQLite and PostgreSQL providers
- DI integration smoke test in `NativeData.AotSmoke` sample
- `docs/di-integration.md` — full guide on context definition, lifecycle, and connection pooling

---

## [0.6.2] — 2026-03-22

### Fixed
- CI/CD and workflow adjustments post-v0.6.1 release

---

## [0.6.1] — 2026-03-22

### Added
- Expression-based `Where(Expression<Func<T, bool>>)` overload on `NativeDataQuery<T>`
  - Supports: `==`, `!=`, `<`, `<=`, `>`, `>=`, `&&`, `||`
  - Throws `NotSupportedException` for unsupported constructs at query-build time
  - Source-generated predicate translation via `ExpressionQueryFilterTranslator<T>` — no `Expression.Compile()` at runtime

---

## [0.6.0] — 2026-03-01

### Added
- **`NativeDataQuery<T>`** fluent query builder in `NativeData.Core`
  - `Where(QueryFilter)`, `OrderBy(QueryOrder)`, `OrderByDescending(QueryOrder)`, `Take(int)`, `Skip(int)`
  - `AsAsyncEnumerable()`, `ToListAsync()`, `FirstOrDefaultAsync()` terminal methods
  - Fully parameterized SQL generation through `ISqlDialect`
- **`SqlRepository<T>.Query()`** factory method returning a `NativeDataQuery<T>`
- Source-generated `{Entity}Filters` and `{Entity}Orders` static helper classes per entity
- `QueryFilter` and `QueryOrder` record structs in `NativeData.Abstractions`

---

## [0.5.2] — 2026-03-01

### Fixed
- Removed AOT publish trimming flags from `release.ps1` that caused publish failures on some targets

---

## [0.5.0] — 2026-02-28

### Added
- **`NativeData.Postgres`** — PostgreSQL provider package
  - `PostgresConnectionFactory` using Npgsql
  - `PostgresSqlDialect` with double-quoted identifiers and `@`-prefixed parameters
  - Full XML documentation
- PostgreSQL integration tests gated on `POSTGRES_INTEGRATION_TEST` environment variable via `FactIfEnvAttribute`
- `docs/providers.md` — compatibility matrix for SQLite and PostgreSQL, AOT/trimming details, provider extension pattern

---

## [0.4.0] — 2026-02-22

### Added
- **`NativeData.Analyzers`** — Roslyn diagnostic analyzer package
  - `ND0001` — warns on `Type.GetType(string)` usage
  - `ND0002` — warns on `Assembly.Load(string)` usage
  - `ND0003` — warns on string-based `Activator.CreateInstance` usage
  - `ND1001` — warns when `[NativeDataEntity]` key column has no matching public property
  - `ND1002` — warns when `[NativeDataEntity]` table name or key column is empty/whitespace
- Analyzer remediation documentation under `docs/analyzers/` (ND0001–ND0003, ND1001–ND1002)
- `NativeData.AnalyzerSmoke` sample validating analyzer rules fire correctly

---

## [0.3.0] — 2026-02-22

### Added
- **`NativeData.Generators`** — Roslyn incremental source generator
  - Triggered by `[NativeDataEntity("TableName", "KeyColumn")]`
  - Emits `IEntityMap<T>` implementation per entity
  - Emits `NativeDataEntityRegistry` with type-to-map entries
  - Emits `NativeDataEntityMaps.Create<T>()` reflection-free factory
  - `NDG0001` diagnostic for unsupported entity shapes
  - `NDG0002` diagnostic for missing key property
- Support for two entity constructor patterns: parameterized constructor, or parameterless + settable properties
- Generator determinism tests (stable output across clean/repeat builds)
- BenchmarkDotNet benchmarks comparing generated vs. manual mapping

---

## [0.2.0] — 2026-02-21

### Added
- CI workflow (`.github/workflows/ci.yml`) with build, test, and AOT smoke publish gates on Ubuntu, Windows, and macOS
- `docs/release-notes-template.md` — standardized release note template
- SQL edge-case tests: parameter prefix normalization (`@`, `:`, `$`), key-only update guard, null parameter values, whitespace `whereClause` handling

### Changed
- Enabled XML documentation output for `NativeData.Abstractions` and `NativeData.Core`
- Added XML doc comments across all public APIs in Abstractions and Core
- Centralized package metadata (`Authors`, `License`, `RepositoryUrl`, `PackageTags`) in `Directory.Build.props`

### Fixed
- `SqlRepository<T>.UpdateAsync` now throws `InvalidOperationException` when the entity map produces no non-key column assignments, preventing silent no-op SQL generation

---

## [0.1.0] — 2026-02-21

### Added
- `NativeData.Abstractions` — provider-agnostic contracts: `IRepository<T>`, `ICommandExecutor`, `ISqlDialect`, `IEntityMap<T>`, `IDbConnectionFactory`, `SqlParameterValue`, `NativeDataEntityAttribute`
- `NativeData.Core` — ADO.NET implementation: `SqlRepository<T>`, `DbCommandExecutor`, `DefaultSqlDialect`
- `NativeData.Sqlite` — SQLite provider: `SqliteConnectionFactory`, `SqliteSqlDialect`
- `NativeData.AotSmoke` — sample project validating Native AOT publish succeeds
- `scripts/release.ps1` and `.github/workflows/release-on-demand.yml` for versioned releases to NuGet.org

[Unreleased]: https://github.com/kylek-dev/NativeData/compare/v0.7.1...HEAD
[0.7.1]: https://github.com/kylek-dev/NativeData/compare/v0.7.0...v0.7.1
[0.7.0]: https://github.com/kylek-dev/NativeData/compare/v0.6.2...v0.7.0
[0.6.2]: https://github.com/kylek-dev/NativeData/compare/v0.6.1...v0.6.2
[0.6.1]: https://github.com/kylek-dev/NativeData/compare/v0.6.0...v0.6.1
[0.6.0]: https://github.com/kylek-dev/NativeData/compare/v0.5.2...v0.6.0
[0.5.2]: https://github.com/kylek-dev/NativeData/compare/v0.5.0...v0.5.2
[0.5.0]: https://github.com/kylek-dev/NativeData/compare/v0.4.1...v0.5.0
[0.4.0]: https://github.com/kylek-dev/NativeData/compare/v0.3.1...v0.4.0
[0.3.0]: https://github.com/kylek-dev/NativeData/compare/v0.2.3...v0.3.0
[0.2.0]: https://github.com/kylek-dev/NativeData/compare/v0.1.1...v0.2.0
[0.1.0]: https://github.com/kylek-dev/NativeData/releases/tag/v0.1.0
