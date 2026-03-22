using Microsoft.Extensions.DependencyInjection;
using NativeData.Abstractions;
using NativeData.Core;
using NativeData.Extensions.DependencyInjection;
using NativeData.Generated;
using NativeData.Sqlite;

var dbPath = Path.Combine(Path.GetTempPath(), $"nativedata-smoke-{Guid.NewGuid():N}.db");
var connectionString = $"Data Source={dbPath}";

var services = new ServiceCollection();
services.AddNativeData<SmokeContext>(o => o.UseSqlite(connectionString));

await using var provider = services.BuildServiceProvider();

// Create the table using the connection factory directly
var factory = provider.GetRequiredService<IDbConnectionFactory>();
var executor = new DbCommandExecutor(factory);
await executor.ExecuteAsync("CREATE TABLE Widgets (Id INTEGER PRIMARY KEY, Name TEXT NOT NULL)");

// Use a scope to exercise scoped context resolution
await using (var scope = provider.CreateAsyncScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SmokeContext>();

    var inserted = await context.Widgets.InsertAsync(new Widget(1, "AOT-Ready"));
    Console.WriteLine($"Rows inserted: {inserted}");

    var loaded = await context.Widgets.GetByIdAsync(1);
    Console.WriteLine(loaded is null ? "No entity returned" : loaded.Name);
}

Console.WriteLine("DI smoke test passed.");

[NativeDataEntity("Widgets", "Id")]
public sealed record Widget(int Id, string Name);

public sealed class SmokeContext : NativeDataContext
{
    public SmokeContext(IDbConnectionFactory connectionFactory, ISqlDialect sqlDialect)
        : base(connectionFactory, sqlDialect)
    {
        RegisterMap(NativeDataEntityMaps.Create<Widget>());
    }

    public IRepository<Widget> Widgets => Repository<Widget>();
}
