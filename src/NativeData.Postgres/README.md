# NativeData.Postgres

PostgreSQL provider package for NativeData.

## Contents

- `PostgresConnectionFactory` — opens `NpgsqlConnection` instances
- `PostgresSqlDialect` — double-quoted identifiers, `@`-prefixed parameters

## Dependencies

- `Npgsql` (NativeAOT-compatible from Npgsql 8+)

## Usage

```csharp
using NativeData.Core;
using NativeData.Postgres;

var connectionFactory = new PostgresConnectionFactory("Host=localhost;Database=mydb;Username=myuser;Password=mypassword");
var executor = new DbCommandExecutor(connectionFactory);
var repository = new SqlRepository<MyEntity>(executor, NativeDataEntityMaps.Create<MyEntity>(), new PostgresSqlDialect());
```

## Build

```bash
dotnet build src/NativeData.Postgres/NativeData.Postgres.csproj
```

## Packaging

This project is packable and published as a NuGet package.
