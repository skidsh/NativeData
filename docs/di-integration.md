---
title: DI Integration
nav_order: 4
---

# DI Integration

NativeData provides first-class support for `Microsoft.Extensions.DependencyInjection` via the `NativeData.Extensions.DependencyInjection` package.

## Installation

```bash
dotnet add package NativeData.Extensions.DependencyInjection
dotnet add package NativeData.Sqlite   # or NativeData.Postgres
```

## Registering services

Call `AddNativeData<TContext>()` during host startup, passing a configuration action that selects a provider:

```csharp
builder.Services.AddNativeData<AppContext>(o => o.UseSqlite("Data Source=app.db"));
// or
builder.Services.AddNativeData<AppContext>(o => o.UsePostgres(connectionString));
```

This registers:

| Service | Lifetime | Notes |
|---------|----------|-------|
| `TContext` | Scoped | One instance per DI scope (request) |
| `IDbConnectionFactory` | Singleton | Shared; underlying driver pool is thread-safe |
| `ISqlDialect` | Singleton | Stateless; safe to share |

## Defining a context

Subclass `NativeDataContext` and expose typed repository properties:

```csharp
public sealed class AppContext : NativeDataContext
{
    public AppContext(IDbConnectionFactory connectionFactory, ISqlDialect sqlDialect)
        : base(connectionFactory, sqlDialect)
    {
        RegisterMap(NativeDataEntityMaps.Create<Product>());
        RegisterMap(NativeDataEntityMaps.Create<Order>());
    }

    public IRepository<Product> Products => Repository<Product>();
    public IRepository<Order> Orders => Repository<Order>();
}
```

## Resolving a context

```csharp
// In a controller, minimal-API handler, or service:
app.MapGet("/products/{id}", async (int id, AppContext db) =>
    await db.Products.GetByIdAsync(id));
```

## Connection lifecycle and pooling

`NativeDataContext` is scoped, meaning one instance is created per DI scope and disposed at the end of that scope. Connections are **not** held open between repository calls — each call opens a connection, executes, and returns it to the pool.

Connection pooling is handled entirely by the underlying ADO.NET driver:

| Provider | Pool implementation | Key configuration |
|----------|---------------------|-------------------|
| SQLite (`Microsoft.Data.Sqlite`) | `SqliteConnectionPool` (per-process, per-connection-string) | `Max Pool Size=N` in the connection string (default: 100). For in-memory databases use a shared cache connection string so the pool shares the same in-memory instance. |
| PostgreSQL (`Npgsql`) | `NpgsqlDataSourcePool` (Npgsql 7+, per data source) | `Maximum Pool Size=N` (default: 100), `Minimum Pool Size=N`. Npgsql 8+ enables multiplexing by default for high-throughput scenarios. |

Because `IDbConnectionFactory` is a singleton, the same pool is shared across all scopes throughout the application lifetime. No additional configuration is required in NativeData itself.

## AOT compatibility

`AddNativeData<TContext>()` is annotated with `[DynamicallyAccessedMembers(PublicConstructors)]` on the `TContext` type parameter, which is the only reflection-based operation. All entity map registration happens through the generated `NativeDataEntityMaps.Create<T>()` factory — no runtime type scanning.
