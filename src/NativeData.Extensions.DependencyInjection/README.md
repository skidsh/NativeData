# NativeData.Extensions.DependencyInjection

`Microsoft.Extensions.DependencyInjection` integration for NativeData.

## Contents

- `AddNativeData<TContext>()` — registers a scoped `NativeDataContext` subclass, a singleton `IDbConnectionFactory`, and a singleton `ISqlDialect`
- `NativeDataOptions` — configuration object passed to provider extension methods (`UseSqlite`, `UsePostgres`)

## Usage

```csharp
builder.Services.AddNativeData<AppContext>(o => o.UseSqlite("Data Source=app.db"));
// or
builder.Services.AddNativeData<AppContext>(o => o.UsePostgres(connectionString));
```

Provider extension methods (`UseSqlite`, `UsePostgres`) are shipped in their respective provider packages — `NativeData.Sqlite` and `NativeData.Postgres`.

## Service lifetimes

| Service | Lifetime |
|---------|----------|
| `TContext` | Scoped |
| `IDbConnectionFactory` | Singleton |
| `ISqlDialect` | Singleton |

## AOT / Trimming

AOT-safe. `AddNativeData<TContext>()` is annotated with `[DynamicallyAccessedMembers(PublicConstructors)]` — the only reflection touch point. Entity map registration uses the generated `NativeDataEntityMaps.Create<T>()` factory with no runtime type scanning.

## Build

```bash
dotnet build src/NativeData.Extensions.DependencyInjection/NativeData.Extensions.DependencyInjection.csproj
```

## Packaging

This project is packable and published as a NuGet package.
