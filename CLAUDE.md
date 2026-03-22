# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What This Project Is

NativeData is an **AOT-first ORM foundation for .NET**. Its core constraint is **NativeAOT/trimming compatibility** — no runtime reflection, no dynamic code generation, no assembly scanning in any hot path. Entity mapping is generated at compile time by a Roslyn incremental source generator.

## Build, Test, and Lint

```bash
# Restore + build (warnings are errors)
dotnet restore NativeData.slnx
dotnet build NativeData.slnx -warnaserror

# Run all tests
dotnet test NativeData.slnx

# Run a single test project
dotnet test tests/NativeData.Tests/NativeData.Tests.csproj

# Run a single test by name
dotnet test tests/NativeData.Tests/NativeData.Tests.csproj --filter "FullyQualifiedName~MyTestName"

# AOT smoke test — run before any runtime/provider changes
dotnet publish samples/NativeData.AotSmoke/NativeData.AotSmoke.csproj -c Release -r win-x64 -p:PublishAot=true -p:PublishTrimmed=true
```

CI gates: build + test + AOT publish smoke. See `.github/workflows/ci.yml`.

## Architecture

```
NativeData.Abstractions   Provider-agnostic contracts (IRepository<T>, ICommandExecutor,
                           ISqlDialect, IEntityMap<T>, IDbConnectionFactory)
NativeData.Core           ADO.NET implementation (SqlRepository<T>, DbCommandExecutor,
                           DefaultSqlDialect, NativeDataContext, NativeDataQuery<T>,
                           ExpressionQueryFilterTranslator<T>)
NativeData.Generators     Roslyn incremental source generator — triggered by [NativeDataEntity]
NativeData.Analyzers      Diagnostic analyzers for AOT/trim safety (rules ND0001–ND1002, ND0004)
NativeData.Sqlite         SQLite provider (SqliteConnectionFactory, SqliteSqlDialect)
NativeData.Postgres       PostgreSQL provider (PostgresConnectionFactory, PostgresSqlDialect) — Npgsql
tests/NativeData.Tests    xUnit test suite covering unit, generator, analyzer, and provider tests
benchmarks/               BenchmarkDotNet perf tests
samples/NativeData.AotSmoke  Smoke test that validates a full NativeAOT publish succeeds
```

### Source Generation Flow

1. Annotate a class/record with `[NativeDataEntity("TableName", "KeyColumn")]`
2. The incremental generator discovers it via `SyntaxProvider`
3. Generator emits:
   - An `IEntityMap<T>` implementation (file-scoped internal class)
   - A `NativeDataEntityRegistry` with type-to-map entries
   - A `NativeDataEntityMaps.Create<T>()` factory for runtime lookup without reflection
   - Filter helpers (`PersonFilters`) and order helpers (`PersonOrders`) per entity
4. Diagnostics `NDG0001`/`NDG0002` fire if the entity shape is invalid

### SQL Generation

SQL is built using `ISqlDialect` for identifier quoting and parameter naming (e.g., `@Col` vs `$Col` depending on provider). `SqlRepository<T>` guards against key-only updates by throwing `InvalidOperationException`. The fluent `NativeDataQuery<T>` builder (accessed via `SqlRepository<T>.Query()`) supports expression-based WHERE translation via `ExpressionQueryFilterTranslator<T>` — supports `==`, `!=`, `<`, `<=`, `>`, `>=`, `&&`, `||`; throws `NotSupportedException` for unsupported constructs.

## Key Conventions

- **AOT/trim safety is non-negotiable.** Never add runtime reflection, `Type.GetType`, dynamic proxies, or assembly scanning in core runtime paths.
- **`ValueTask<T>` and `IAsyncEnumerable<T>`** for all async I/O — not `Task<T>`.
- **Nullable reference types are enabled project-wide** (`TreatWarningsAsErrors` is on).
- **New providers** go in their own project (`NativeData.{Provider}`) implementing `IDbConnectionFactory` and `ISqlDialect`. Add shared SQL-generation behavioral tests using `[Theory]` + `[MemberData]` so both dialects are covered without a live DB.
- **Generator tests** compile source strings with Roslyn (`CSharpCompilation`) and assert on generated output strings and emitted diagnostics. Follow the triple-A pattern in `GeneratorTests.cs`.
- **Analyzer tests** validate rule IDs and positive/negative cases per rule.
- **PostgreSQL integration tests** are gated on the `POSTGRES_INTEGRATION_TEST` environment variable via `FactIfEnvAttribute`.

## Out of Scope (Planned Future Milestones)

- Migrations
- Change tracking / identity map
- Full LINQ translation (current expression-to-SQL support is a bounded subset)

## GitHub PR Workflow

- Always create a **new branch** for changes; do not work directly on `main`.
- Before opening/updating a PR, sync by pulling `main` and merging `main` into the feature branch.
- In PR descriptions, use one issue-closing keyword per line: `Closes #20`
