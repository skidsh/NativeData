using System.Collections.Generic;
using System.Data;
using NativeData.Abstractions;
using NativeData.Core;
using NativeData.Sqlite;

namespace NativeData.Tests;

public class SqlRepositoryTests
{
    [Fact]
    public async Task Insert_BuildsExpectedCommand()
    {
        var executor = new RecordingCommandExecutor();
        var repository = new SqlRepository<TestEntity>(executor, new TestEntityMap());

        await repository.InsertAsync(new TestEntity(7, "native"));

        Assert.Equal("INSERT INTO [TestEntities] ([Id], [Name]) VALUES (@Id, @Name)", executor.LastCommandText);
        Assert.Equal(2, executor.LastParameters?.Count);
    }

    [Fact]
    public async Task DeleteById_BuildsExpectedCommand()
    {
        var executor = new RecordingCommandExecutor();
        var repository = new SqlRepository<TestEntity>(executor, new TestEntityMap());

        await repository.DeleteByIdAsync(42);

        Assert.Equal("DELETE FROM [TestEntities] WHERE [Id] = @Id", executor.LastCommandText);
        Assert.Single(executor.LastParameters!);
    }

    [Fact]
    public async Task Insert_NormalizesPrefixedParameterNames()
    {
        var executor = new RecordingCommandExecutor();
        var repository = new SqlRepository<TestEntity>(executor, new PrefixedParameterEntityMap());

        await repository.InsertAsync(new TestEntity(7, "native"));

        Assert.Equal("INSERT INTO [TestEntities] ([Id], [Name]) VALUES (@Id, @Name)", executor.LastCommandText);
        Assert.Equal(2, executor.LastParameters?.Count);
    }

    [Fact]
    public async Task Update_WithOnlyKeyParameter_ThrowsInvalidOperationException()
    {
        var executor = new RecordingCommandExecutor();
        var repository = new SqlRepository<TestEntity>(executor, new KeyOnlyUpdateEntityMap());

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await repository.UpdateAsync(new TestEntity(7, "native")));
    }

    [Fact]
    public async Task Insert_AllowsNullParameterValue()
    {
        var executor = new RecordingCommandExecutor();
        var repository = new SqlRepository<TestEntity>(executor, new TestEntityMap());

        await repository.InsertAsync(new TestEntity(7, null!));

        Assert.Equal("INSERT INTO [TestEntities] ([Id], [Name]) VALUES (@Id, @Name)", executor.LastCommandText);
        Assert.Null(executor.LastParameters?[1].Value);
    }

    [Fact]
    public async Task Query_WithWhitespaceWhereClause_DoesNotAppendWhere()
    {
        var executor = new RecordingCommandExecutor();
        var repository = new SqlRepository<TestEntity>(executor, new TestEntityMap());

        await foreach (var _ in repository.QueryAsync("   "))
        {
        }

        Assert.Equal("SELECT * FROM [TestEntities]", executor.LastCommandText);
    }

    [Fact]
    public async Task SqliteProvider_RoundTripsEntity()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"nativedata-test-{Guid.NewGuid():N}.db");

        try
        {
            var connectionFactory = new SqliteConnectionFactory($"Data Source={dbPath};Pooling=False");
            var executor = new DbCommandExecutor(connectionFactory);
            var repository = new SqlRepository<TestEntity>(executor, new TestEntityMap(), new SqliteSqlDialect());

            await executor.ExecuteAsync("CREATE TABLE TestEntities (Id INTEGER PRIMARY KEY, Name TEXT NOT NULL)");
            await repository.InsertAsync(new TestEntity(12, "sqlite"));

            var loaded = await repository.GetByIdAsync(12);

            Assert.NotNull(loaded);
            Assert.Equal("sqlite", loaded!.Name);
        }
        finally
        {
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
        }
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

    private sealed class PrefixedParameterEntityMap : IEntityMap<TestEntity>
    {
        public string TableName => "TestEntities";

        public string KeyColumn => "Id";

        public IReadOnlyList<string> WritableColumns => ["Id", "Name"];

        public object? GetKey(TestEntity entity) => entity.Id;

        public IReadOnlyList<SqlParameterValue> BuildInsertParameters(TestEntity entity)
            => [new("@Id", entity.Id), new(":Name", entity.Name)];

        public IReadOnlyList<SqlParameterValue> BuildUpdateParameters(TestEntity entity)
            => [new("$Id", entity.Id), new(":Name", entity.Name)];

        public TestEntity Materialize(IDataRecord record)
            => new(record.GetInt32(0), record.GetString(1));
    }

    private sealed class KeyOnlyUpdateEntityMap : IEntityMap<TestEntity>
    {
        public string TableName => "TestEntities";

        public string KeyColumn => "Id";

        public IReadOnlyList<string> WritableColumns => ["Id", "Name"];

        public object? GetKey(TestEntity entity) => entity.Id;

        public IReadOnlyList<SqlParameterValue> BuildInsertParameters(TestEntity entity)
            => [new("Id", entity.Id), new("Name", entity.Name)];

        public IReadOnlyList<SqlParameterValue> BuildUpdateParameters(TestEntity entity)
            => [new("Id", entity.Id)];

        public TestEntity Materialize(IDataRecord record)
            => new(record.GetInt32(0), record.GetString(1));
    }
}
