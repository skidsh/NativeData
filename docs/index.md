---
title: Home
nav_order: 1
---

# NativeData

NativeData is an **AOT-first ORM foundation** for .NET 10, designed for applications that require Native AOT compilation and trimming compatibility.

[![CI](https://github.com/kylek-dev/NativeData/actions/workflows/ci.yml/badge.svg)](https://github.com/kylek-dev/NativeData/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/NativeData.Core.svg)](https://www.nuget.org/packages?q=NativeData)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/kylek-dev/NativeData/blob/main/LICENSE)

## Why NativeData?

Modern .NET applications increasingly target Native AOT and trimmed publish scenarios, where reflection-heavy ORMs fail at runtime. NativeData solves this by favoring:

- **Compile-time metadata** over runtime reflection
- **Explicit provider wiring** over dynamic provider loading
- **Predictable SQL generation** over broad dynamic conventions

## Packages

| Package | Description |
|---------|-------------|
| `NativeData.Abstractions` | Core contracts and primitives (`IRepository<T>`, `IEntityMap<T>`, `ISqlDialect`) |
| `NativeData.Core` | Runtime implementations (`SqlRepository<T>`, `DbCommandExecutor`, `NativeDataContext`) |
| `NativeData.Sqlite` | SQLite provider (`SqliteConnectionFactory`, `SqliteSqlDialect`, `UseSqlite`) |
| `NativeData.Postgres` | PostgreSQL provider (`PostgresConnectionFactory`, `PostgresSqlDialect`, `UsePostgres`) |
| `NativeData.Extensions.DependencyInjection` | `AddNativeData<TContext>()` for `Microsoft.Extensions.DependencyInjection` |
| `NativeData.Generators` | Source generator for `[NativeDataEntity]` annotated types |
| `NativeData.Analyzers` | Roslyn analyzer for trim/AOT safety checks |

## Quick Start

### Install packages

```bash
dotnet add package NativeData.Core
dotnet add package NativeData.Sqlite
```

### Define an entity

```csharp
using NativeData.Abstractions;

[NativeDataEntity]
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
}
```

### Configure with DI (recommended)

```csharp
using NativeData.Extensions.DependencyInjection;
using NativeData.Sqlite;

builder.Services.AddNativeData<AppContext>(o => o.UseSqlite("Data Source=myapp.db"));

// In a handler or service:
app.MapGet("/products/{id}", async (int id, AppContext db) =>
    await db.Products.GetByIdAsync(id));
```

See [DI Integration](di-integration) for full details on context definition, scoped lifecycle, and connection pooling.

### Configure without DI

```csharp
using NativeData.Core;
using NativeData.Sqlite;

var factory = new SqliteConnectionFactory("Data Source=myapp.db");
var executor = new DbCommandExecutor(factory);
var dialect = new SqliteSqlDialect();

var repo = new SqlRepository<Product>(executor, dialect, new ProductEntityMap());

await repo.InsertAsync(new Product { Id = 1, Name = "Widget", Price = 9.99m });
var product = await repo.GetByIdAsync(1);
```

## Capabilities

- Repository-style API (`IRepository<T>`) with `GetByIdAsync`, `InsertAsync`, `UpdateAsync`, `DeleteByIdAsync`
- Fluent query builder (`NativeDataQuery<T>`) with expression-based `Where`, `OrderBy`, `Take`, `Skip`
  - Source-generated predicate translation — no `Expression.Compile()` at runtime
  - Supported operators: `==`, `!=`, `<`, `<=`, `>`, `>=`, `&&`, `||`
- Provider-agnostic ADO.NET execution via `ICommandExecutor` / `ISqlDialect`
- SQLite and PostgreSQL provider packages
- DI integration via `AddNativeData<TContext>()` with scoped context and singleton connection factory
- Roslyn source generator for `[NativeDataEntity]` — emits `IEntityMap<T>`, filter/order helpers at compile time
- Roslyn analyzer pack — 6 rules covering trim/AOT safety and entity mapping validation (ND0001–ND0004, ND1001–ND1002)

## Out of Scope

- Migrations
- Change tracking / identity map
- Full LINQ translation (current subset is intentional for AOT safety)

## Documentation

- [Getting Started](getting-started)
- [DI Integration](di-integration)
- [Provider compatibility](providers)
- [Project status and roadmap](status-and-roadmap)
- [Release checklist](release-checklist)
- [Contributing guide](https://github.com/kylek-dev/NativeData/blob/main/CONTRIBUTING.md)

## Build from Source

```bash
dotnet restore NativeData.slnx
dotnet build NativeData.slnx -warnaserror
dotnet test NativeData.slnx
```

## License

NativeData is open source under the [MIT License](https://github.com/kylek-dev/NativeData/blob/main/LICENSE).
