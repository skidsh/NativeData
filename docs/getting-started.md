---
title: Getting Started
nav_order: 2
---

# Getting Started

This guide walks you through installing NativeData, defining your first entity, and running queries — with or without dependency injection.

## Prerequisites

- .NET 10 SDK or later
- A supported database: SQLite or PostgreSQL

---

## 1. Install packages

Choose the provider that matches your database. You always need the provider package; `NativeData.Core` and `NativeData.Abstractions` are pulled in transitively.

### SQLite

```bash
dotnet add package NativeData.Sqlite
dotnet add package NativeData.Generators
dotnet add package NativeData.Analyzers
```

### PostgreSQL

```bash
dotnet add package NativeData.Postgres
dotnet add package NativeData.Generators
dotnet add package NativeData.Analyzers
```

### Optional: dependency injection integration

```bash
dotnet add package NativeData.Extensions.DependencyInjection
```

---

## 2. Define an entity

Annotate a class or record with `[NativeDataEntity]`, providing the table name and (optionally) the key column name. The source generator picks this up at compile time and emits all mapping code — no reflection at runtime.

```csharp
using NativeData.Abstractions;

[NativeDataEntity("Products", "Id")]
public sealed class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
}
```

The generator emits:
- `IEntityMap<Product>` — column-to-property mapping
- `NativeDataEntityMaps.Create<Product>()` — reflection-free factory
- `ProductFilters` — static helpers for building `Where` clauses
- `ProductOrders` — static helpers for building `OrderBy` clauses

> If the entity shape is unsupported or the key column doesn't match a property, the `NativeData.Analyzers` package emits build-time diagnostics (`ND1001`, `ND1002`) rather than failing at runtime.

---

## 3a. Configure with DI (recommended)

This approach works well with ASP.NET Core, Worker Services, and any host using `Microsoft.Extensions.DependencyInjection`.

### Register services

```csharp
using NativeData.Extensions.DependencyInjection;
using NativeData.Sqlite;  // or NativeData.Postgres

// In Program.cs / Startup.cs:
builder.Services.AddNativeData<AppDbContext>(o => o.UseSqlite("Data Source=app.db"));
// or for PostgreSQL:
// builder.Services.AddNativeData<AppDbContext>(o => o.UsePostgres("Host=localhost;Database=mydb;Username=app;Password=secret"));
```

This registers `AppDbContext` as **Scoped**, and `IDbConnectionFactory` / `ISqlDialect` as **Singletons**.

### Define a context

Subclass `NativeDataContext` and expose your entities as typed repository properties:

```csharp
using NativeData.Abstractions;
using NativeData.Core;

public sealed class AppDbContext : NativeDataContext
{
    public AppDbContext(IDbConnectionFactory connectionFactory, ISqlDialect dialect)
        : base(connectionFactory, dialect)
    {
        RegisterMap(NativeDataEntityMaps.Create<Product>());
    }

    public IRepository<Product> Products => Repository<Product>();
}
```

### Use the context

```csharp
// Minimal API
app.MapGet("/products/{id}", async (int id, AppDbContext db) =>
    await db.Products.GetByIdAsync(id));

app.MapPost("/products", async (Product product, AppDbContext db) =>
{
    await db.Products.InsertAsync(product);
    return Results.Created($"/products/{product.Id}", product);
});
```

---

## 3b. Configure without DI

For console apps, tests, or scenarios without a DI container:

```csharp
using NativeData.Core;
using NativeData.Sqlite;

var factory = new SqliteConnectionFactory("Data Source=app.db");
var executor = new DbCommandExecutor(factory);
var dialect = new SqliteSqlDialect();
var map = NativeDataEntityMaps.Create<Product>();

var repo = new SqlRepository<Product>(executor, map, dialect);
```

---

## 4. CRUD operations

All repository methods are async and return `ValueTask<T>` or `IAsyncEnumerable<T>`.

```csharp
// Insert
await repo.InsertAsync(new Product { Id = 1, Name = "Widget", Price = 9.99m });

// Get by primary key
Product? product = await repo.GetByIdAsync(1);

// Update
product!.Price = 12.99m;
await repo.UpdateAsync(product);

// Delete
await repo.DeleteByIdAsync(1);

// Get all
List<Product> all = await repo.GetAllToListAsync();
```

---

## 5. Query builder

`SqlRepository<T>.Query()` returns a `NativeDataQuery<T>` builder for filtering, ordering, and paging.

### Expression-based WHERE (source-generated, AOT-safe)

```csharp
List<Product> cheap = await repo.Query()
    .Where(p => p.Price < 10m)
    .ToListAsync();
```

Supported operators: `==`, `!=`, `<`, `<=`, `>`, `>=`, `&&`, `||`, and parentheses.
Unsupported constructs (method calls, string methods, etc.) throw `NotSupportedException` at query-build time.

### Generated filter helpers

The source generator emits a `ProductFilters` static class with typed `QueryFilter` factories:

```csharp
using static ProductFilters;

List<Product> results = await repo.Query()
    .Where(ByName("Widget"))
    .OrderBy(ProductOrders.ByPrice())
    .Take(10)
    .ToListAsync();
```

### Raw SQL filter

You can also pass raw SQL when you need full control:

```csharp
var filter = new QueryFilter { Sql = "Price < @maxPrice", Parameters = [new SqlParameterValue("maxPrice", 10m)] };
List<Product> results = await repo.Query().Where(filter).ToListAsync();
```

### Pagination

```csharp
List<Product> page = await repo.Query()
    .OrderBy(ProductOrders.ById())
    .Skip(20)
    .Take(10)
    .ToListAsync();
```

---

## 6. Analyzer diagnostics

The `NativeData.Analyzers` package ships two categories of build-time rules:

**Trim/AOT safety** — warn when your code uses reflection-heavy APIs:

| Rule | Pattern flagged |
|------|----------------|
| ND0001 | `Type.GetType(string)` |
| ND0002 | `Assembly.Load(string)` |
| ND0003 | `Activator.CreateInstance(string, ...)` |
| ND0004 | `Expression.Compile()` / `LambdaExpression.Compile()` |

**Entity mapping validation**:

| Rule | Condition |
|------|-----------|
| ND1001 | Key column declared in `[NativeDataEntity]` has no matching public property |
| ND1002 | Table name or key column argument is empty or whitespace |

Diagnostics fire as **warnings** during `dotnet build`. They do not block compilation unless your project sets `TreatWarningsAsErrors`.

---

## Next steps

- [DI Integration](di-integration) — full context lifecycle and connection pooling details
- [Provider Compatibility](providers) — SQLite and PostgreSQL dialect details, adding a new provider
- [Project status and roadmap](status-and-roadmap)
