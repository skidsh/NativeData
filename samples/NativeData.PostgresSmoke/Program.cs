using NativeData.Abstractions;
using NativeData.Core;
using NativeData.Generated;
using NativeData.Postgres;

var connectionString = args.Length > 0
    ? args[0]
    : Environment.GetEnvironmentVariable("NATIVEDATA_POSTGRES_CONNECTION")
      ?? "Host=localhost;Database=nativedata_smoke;Username=postgres;Password=postgres";

var connectionFactory = new PostgresConnectionFactory(connectionString);
var executor = new DbCommandExecutor(connectionFactory);
var repository = new SqlRepository<Widget>(executor, NativeDataEntityMaps.Create<Widget>(), new PostgresSqlDialect());

await executor.ExecuteAsync("CREATE TABLE IF NOT EXISTS Widgets (\"Id\" SERIAL PRIMARY KEY, \"Name\" TEXT NOT NULL)");

var inserted = await repository.InsertAsync(new Widget(0, "Postgres-Ready"));
Console.WriteLine($"Rows inserted: {inserted}");

var results = new List<Widget>();
await foreach (var w in repository.QueryAsync())
    results.Add(w);

Console.WriteLine($"Rows loaded: {results.Count}");
if (results.Count > 0)
    Console.WriteLine($"Last name: {results[^1].Name}");

[NativeDataEntity("Widgets", "Id")]
public sealed record Widget(int Id, string Name);
