using System.Collections.Generic;
using System.Data;
using NativeData.Abstractions;
using NativeData.Core;
using NativeData.Postgres;
using NativeData.Sqlite;

namespace NativeData.Tests;

public class PostgresProviderTests
{
    // Shared behavioral data: both dialects use double-quoted identifiers and @-prefixed parameters.
    public static IEnumerable<object[]> DoubleQuoteDialects()
    {
        yield return [new SqliteSqlDialect()];
        yield return [new PostgresSqlDialect()];
    }

    [Theory]
    [MemberData(nameof(DoubleQuoteDialects))]
    public async Task Insert_WithDoubleQuoteDialect_BuildsExpectedCommand(ISqlDialect dialect)
    {
        var executor = new RecordingCommandExecutor();
        var repository = new SqlRepository<TestEntity>(executor, new TestEntityMap(), dialect);

        await repository.InsertAsync(new TestEntity(1, "postgres"));

        Assert.Equal("INSERT INTO \"TestEntities\" (\"Id\", \"Name\") VALUES (@Id, @Name)", executor.LastCommandText);
    }

    [Theory]
    [MemberData(nameof(DoubleQuoteDialects))]
    public async Task Delete_WithDoubleQuoteDialect_BuildsExpectedCommand(ISqlDialect dialect)
    {
        var executor = new RecordingCommandExecutor();
        var repository = new SqlRepository<TestEntity>(executor, new TestEntityMap(), dialect);

        await repository.DeleteByIdAsync(99);

        Assert.Equal("DELETE FROM \"TestEntities\" WHERE \"Id\" = @Id", executor.LastCommandText);
    }

    [Theory]
    [MemberData(nameof(DoubleQuoteDialects))]
    public async Task Update_WithDoubleQuoteDialect_BuildsExpectedCommand(ISqlDialect dialect)
    {
        var executor = new RecordingCommandExecutor();
        var repository = new SqlRepository<TestEntity>(executor, new TestEntityMap(), dialect);

        await repository.UpdateAsync(new TestEntity(1, "updated"));

        Assert.Equal("UPDATE \"TestEntities\" SET \"Name\" = @Name WHERE \"Id\" = @Id", executor.LastCommandText);
    }

    [Theory]
    [MemberData(nameof(DoubleQuoteDialects))]
    public async Task Query_WithDoubleQuoteDialect_BuildsExpectedCommand(ISqlDialect dialect)
    {
        var executor = new RecordingCommandExecutor();
        var repository = new SqlRepository<TestEntity>(executor, new TestEntityMap(), dialect);

        await foreach (var _ in repository.QueryAsync()) { }

        Assert.Equal("SELECT * FROM \"TestEntities\"", executor.LastCommandText);
    }

    [Fact]
    public void PostgresSqlDialect_QuoteIdentifier_UsesDoubleQuotes()
    {
        var dialect = new PostgresSqlDialect();
        Assert.Equal("\"MyTable\"", dialect.QuoteIdentifier("MyTable"));
    }

    [Fact]
    public void PostgresSqlDialect_BuildParameterName_UsesAtPrefix()
    {
        var dialect = new PostgresSqlDialect();
        Assert.Equal("@Id", dialect.BuildParameterName("Id"));
        Assert.Equal("@Id", dialect.BuildParameterName("@Id"));
        Assert.Equal("@Id", dialect.BuildParameterName(":Id"));
        Assert.Equal("@Id", dialect.BuildParameterName("$Id"));
    }

    [Fact]
    public void PostgresSqlDialect_NormalizeParameterName_StripsAllPrefixes()
    {
        var dialect = new PostgresSqlDialect();
        Assert.Equal("Id", dialect.NormalizeParameterName("@Id"));
        Assert.Equal("Id", dialect.NormalizeParameterName(":Id"));
        Assert.Equal("Id", dialect.NormalizeParameterName("$Id"));
        Assert.Equal("Id", dialect.NormalizeParameterName("Id"));
    }

    [Fact(Skip = "Requires live PostgreSQL — set NATIVEDATA_POSTGRES_CONNECTION to run")]
    public async Task PostgresProvider_RoundTripsEntity()
    {
        var connectionString = Environment.GetEnvironmentVariable("NATIVEDATA_POSTGRES_CONNECTION")
            ?? "Host=localhost;Database=nativedata_test;Username=postgres;Password=postgres";

        var connectionFactory = new PostgresConnectionFactory(connectionString);
        var executor = new DbCommandExecutor(connectionFactory);
        var repository = new SqlRepository<TestEntity>(executor, new TestEntityMap(), new PostgresSqlDialect());

        await executor.ExecuteAsync("CREATE TABLE IF NOT EXISTS \"TestEntities\" (\"Id\" INTEGER PRIMARY KEY, \"Name\" TEXT NOT NULL)");
        await repository.InsertAsync(new TestEntity(42, "postgres"));

        var loaded = await repository.GetByIdAsync(42);

        Assert.NotNull(loaded);
        Assert.Equal("postgres", loaded!.Name);

        await executor.ExecuteAsync("DROP TABLE IF EXISTS \"TestEntities\"");
    }

    private sealed class RecordingCommandExecutor : ICommandExecutor
    {
        public string? LastCommandText { get; private set; }
        public IReadOnlyList<SqlParameterValue>? LastParameters { get; private set; }

        public ValueTask<int> ExecuteAsync(
            string commandText,
            IReadOnlyList<SqlParameterValue>? parameters = null,
            CancellationToken cancellationToken = default)
        {
            LastCommandText = commandText;
            LastParameters = parameters;
            return ValueTask.FromResult(1);
        }

        public async IAsyncEnumerable<T> QueryAsync<T>(
            string commandText,
            Func<IDataRecord, T> materializer,
            IReadOnlyList<SqlParameterValue>? parameters = null,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            LastCommandText = commandText;
            LastParameters = parameters;
            await Task.CompletedTask;
            yield break;
        }
    }

    private sealed record TestEntity(int Id, string Name);

    private sealed class TestEntityMap : IEntityMap<TestEntity>
    {
        public string TableName => "TestEntities";
        public string KeyColumn => "Id";
        public IReadOnlyList<string> WritableColumns => ["Id", "Name"];
        public object? GetKey(TestEntity entity) => entity.Id;
        public IReadOnlyList<SqlParameterValue> BuildInsertParameters(TestEntity entity)
            => [new("Id", entity.Id), new("Name", entity.Name)];
        public IReadOnlyList<SqlParameterValue> BuildUpdateParameters(TestEntity entity)
            => [new("Id", entity.Id), new("Name", entity.Name)];
        public TestEntity Materialize(IDataRecord record)
            => new(record.GetInt32(0), record.GetString(1));
    }
}
