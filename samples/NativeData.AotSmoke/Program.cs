using System.Collections.Generic;
using System.Data;
using NativeData.Abstractions;
using NativeData.Core;
using NativeData.Sqlite;

var dbPath = Path.Combine(Path.GetTempPath(), $"nativedata-smoke-{Guid.NewGuid():N}.db");
var connectionString = $"Data Source={dbPath}";

var connectionFactory = new SqliteConnectionFactory(connectionString);
var executor = new DbCommandExecutor(connectionFactory);
var repository = new SqlRepository<Widget>(executor, new WidgetMap(), new SqliteSqlDialect());

await executor.ExecuteAsync("CREATE TABLE Widgets (Id INTEGER PRIMARY KEY, Name TEXT NOT NULL)");

var inserted = await repository.InsertAsync(new Widget(1, "AOT-Ready"));
Console.WriteLine($"Rows inserted: {inserted}");

var loaded = await repository.GetByIdAsync(1);
Console.WriteLine(loaded is null ? "No entity returned" : loaded.Name);

public sealed record Widget(int Id, string Name);

public sealed class WidgetMap : IEntityMap<Widget>
{
	public string TableName => "Widgets";

	public string KeyColumn => "Id";

	public IReadOnlyList<string> WritableColumns => ["Id", "Name"];

	public object? GetKey(Widget entity) => entity.Id;

	public IReadOnlyList<SqlParameterValue> BuildInsertParameters(Widget entity)
		=> [new("Id", entity.Id), new("Name", entity.Name)];

	public IReadOnlyList<SqlParameterValue> BuildUpdateParameters(Widget entity)
		=> [new("Id", entity.Id), new("Name", entity.Name)];

	public Widget Materialize(IDataRecord record)
		=> new(record.GetInt32(0), record.GetString(1));
}
