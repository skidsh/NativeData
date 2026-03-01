# NativeData.PostgresSmoke

Smoke test sample for the `NativeData.Postgres` provider.

**Requires a running PostgreSQL instance.**

## Run

```bash
# With connection string as argument
dotnet run --project samples/NativeData.PostgresSmoke -- "Host=localhost;Database=nativedata_smoke;Username=postgres;Password=postgres"

# Or via environment variable
$env:NATIVEDATA_POSTGRES_CONNECTION = "Host=localhost;Database=nativedata_smoke;Username=postgres;Password=postgres"
dotnet run --project samples/NativeData.PostgresSmoke
```

## What it does

1. Opens a connection using `PostgresConnectionFactory`
2. Creates a `Widgets` table (if not exists)
3. Inserts a `Widget` entity using the source-generated map
4. Queries all rows and prints the count and last inserted name
