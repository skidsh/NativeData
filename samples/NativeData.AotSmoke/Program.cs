using NativeData.Abstractions;
using NativeData.Core;
using NativeData.Generated;
using NativeData.Sqlite;

var dbPath = Path.Combine(Path.GetTempPath(), $"nativedata-smoke-{Guid.NewGuid():N}.db");
var connectionString = $"Data Source={dbPath}";

var connectionFactory = new SqliteConnectionFactory(connectionString);
var executor = new DbCommandExecutor(connectionFactory);
var repository = new SqlRepository<Widget>(executor, NativeDataEntityMaps.Create<Widget>(), new SqliteSqlDialect());

await executor.ExecuteAsync("CREATE TABLE Widgets (Id INTEGER PRIMARY KEY, Name TEXT NOT NULL)");

var inserted = await repository.InsertAsync(new Widget(1, "AOT-Ready"));
Console.WriteLine($"Rows inserted: {inserted}");

var loaded = await repository.GetByIdAsync(1);
Console.WriteLine(loaded is null ? "No entity returned" : loaded.Name);

[NativeDataEntity("Widgets", "Id")]
public sealed record Widget(int Id, string Name);
