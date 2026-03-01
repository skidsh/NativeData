---
title: Provider Compatibility Matrix
nav_order: 5
---

# Provider Compatibility Matrix

This document lists available NativeData provider packages and their compatibility characteristics.

## Providers

| Provider | Package | Connection Class | Identifier Quoting | Parameter Prefix | Status |
|---|---|---|---|---|---|
| SQLite | `NativeData.Sqlite` | `SqliteConnectionFactory` | `"identifier"` | `@param` | ✅ Stable |
| PostgreSQL | `NativeData.Postgres` | `PostgresConnectionFactory` | `"identifier"` | `@param` | ✅ Stable |

## Dialect Details

### NativeData.Sqlite

- **Underlying driver:** `Microsoft.Data.Sqlite`
- **Dialect class:** `SqliteSqlDialect`
- **Identifier quoting:** double quotes — `"TableName"`
- **Parameter style:** `@ParamName`

### NativeData.Postgres

- **Underlying driver:** `Npgsql`
- **Dialect class:** `PostgresSqlDialect`
- **Identifier quoting:** double quotes — `"TableName"`
- **Parameter style:** `@ParamName`

## AOT / Trimming Compatibility

All provider packages set `IsAotCompatible` and `IsTrimmable` via `Directory.Build.props`. Both `SqliteConnectionFactory` and `PostgresConnectionFactory` use only ADO.NET abstractions (`DbConnection`) with no runtime reflection.

Npgsql 8+ includes native AOT support. For AOT publish, ensure your application targets `net8.0` or later and uses `NpgsqlDataSource` or basic connection APIs only.

## Adding a New Provider

1. Create `src/NativeData.{Provider}/` with:
   - `{Provider}ConnectionFactory : IDbConnectionFactory`
   - `{Provider}SqlDialect : ISqlDialect`
2. Reference `NativeData.Core` and `NativeData.Abstractions`
3. Add to `NativeData.slnx`
4. Add shared behavioral tests in `NativeData.Tests`
