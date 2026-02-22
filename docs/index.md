---
title: Home
nav_order: 1
---

# NativeData

NativeData is an **AOT-first ORM foundation** for .NET 10, designed for applications that require Native AOT compilation and trimming compatibility.

[![CI](https://github.com/skidsh/NativeData/actions/workflows/ci.yml/badge.svg)](https://github.com/skidsh/NativeData/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/NativeData.Core.svg)](https://www.nuget.org/packages?q=NativeData)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/skidsh/NativeData/blob/main/LICENSE)

## Why NativeData?

Modern .NET applications increasingly target Native AOT and trimmed publish scenarios, where reflection-heavy ORMs fail at runtime. NativeData solves this by favoring:

- **Compile-time metadata** over runtime reflection
- **Explicit provider wiring** over dynamic provider loading
- **Predictable SQL generation** over broad dynamic conventions

## Packages

| Package | Description |
|---------|-------------|
| `NativeData.Abstractions` | Core contracts and primitives (`IRepository<T>`, `IEntityMap<T>`, `ISqlDialect`) |
| `NativeData.Core` | Runtime implementations (`SqlRepository<T>`, `DbCommandExecutor`) |
| `NativeData.Sqlite` | SQLite provider (`SqliteConnectionFactory`, `SqliteSqlDialect`) |
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

### Configure and use the repository

```csharp
using NativeData.Core;
using NativeData.Sqlite;

var factory = new SqliteConnectionFactory("Data Source=myapp.db");
var executor = new DbCommandExecutor(factory);
var dialect = new SqliteSqlDialect();
var map = new ProductEntityMap();

var repo = new SqlRepository<Product>(executor, dialect, map);

await repo.InsertAsync(new Product { Id = 1, Name = "Widget", Price = 9.99m });
var product = await repo.GetByIdAsync(1);
```

## Current Capabilities (MVP)

- Repository-style API (`IRepository<T>`)
- CRUD operations: `GetByIdAsync`, `QueryAsync`, `InsertAsync`, `UpdateAsync`, `DeleteByIdAsync`
- Provider-agnostic execution via ADO.NET abstractions
- Dialect abstraction for identifier quoting and parameter normalization
- SQLite provider package
- Source generator scaffold for `[NativeDataEntity]`
- Roslyn analyzer for trim safety (`ND0001`)

## Out of Scope (MVP)

- Migrations
- Change tracking / identity map
- LINQ translation/provider

## Documentation

- [Project status and roadmap](status-and-roadmap)
- [Release checklist](release-checklist)
- [Contributing guide](https://github.com/skidsh/NativeData/blob/main/CONTRIBUTING.md)

## Build from Source

```bash
dotnet restore NativeData.slnx
dotnet build NativeData.slnx -warnaserror
dotnet test NativeData.slnx
```

## License

NativeData is open source under the [MIT License](https://github.com/skidsh/NativeData/blob/main/LICENSE).
