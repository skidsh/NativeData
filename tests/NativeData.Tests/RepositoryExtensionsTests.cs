using System.Collections.Generic;
using System.Data;
using NativeData.Abstractions;
using NativeData.Core;

namespace NativeData.Tests;

public class RepositoryExtensionsTests
{
    [Fact]
    public async Task ToListAsync_ReturnsAllEntities()
    {
        var executor = new SeededQueryExecutor([new TestEntity(1, "a"), new TestEntity(2, "b")]);
        var repository = new SqlRepository<TestEntity>(executor, new TestEntityMap());

        var result = await repository.GetAllToListAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("a", result[0].Name);
        Assert.Equal("b", result[1].Name);
    }

    [Fact]
    public async Task ToListAsync_ReturnsEmptyList_WhenNoRows()
    {
        var executor = new SeededQueryExecutor([]);
        var repository = new SqlRepository<TestEntity>(executor, new TestEntityMap());

        var result = await repository.GetAllToListAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task QueryToListAsync_PassesThroughWhereClause()
    {
        var executor = new SeededQueryExecutor([new TestEntity(1, "match")]);
        var repository = new SqlRepository<TestEntity>(executor, new TestEntityMap());

        var result = await repository.QueryToListAsync("\"Name\" = @Name", [new SqlParameterValue("Name", "match")]);

        Assert.Single(result);
        Assert.Equal("match", result[0].Name);
        Assert.Contains("WHERE", executor.LastCommandText);
    }

    [Fact]
    public async Task FirstOrDefaultAsync_ReturnsFirst()
    {
        var executor = new SeededQueryExecutor([new TestEntity(1, "first"), new TestEntity(2, "second")]);
        var repository = new SqlRepository<TestEntity>(executor, new TestEntityMap());

        var result = await repository.FirstOrDefaultAsync();

        Assert.NotNull(result);
        Assert.Equal("first", result!.Name);
    }

    [Fact]
    public async Task FirstOrDefaultAsync_ReturnsNull_WhenNoRows()
    {
        var executor = new SeededQueryExecutor([]);
        var repository = new SqlRepository<TestEntity>(executor, new TestEntityMap());

        var result = await repository.FirstOrDefaultAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task AnyAsync_ReturnsTrue_WhenRowsExist()
    {
        var executor = new SeededQueryExecutor([new TestEntity(1, "x")]);
        var repository = new SqlRepository<TestEntity>(executor, new TestEntityMap());

        Assert.True(await repository.AnyAsync());
    }

    [Fact]
    public async Task AnyAsync_ReturnsFalse_WhenNoRows()
    {
        var executor = new SeededQueryExecutor([]);
        var repository = new SqlRepository<TestEntity>(executor, new TestEntityMap());

        Assert.False(await repository.AnyAsync());
    }

    [Fact]
    public async Task CountAsync_ReturnsCorrectCount()
    {
        var executor = new SeededQueryExecutor([new TestEntity(1, "a"), new TestEntity(2, "b"), new TestEntity(3, "c")]);
        var repository = new SqlRepository<TestEntity>(executor, new TestEntityMap());

        Assert.Equal(3, await repository.CountAsync());
    }

    private sealed class SeededQueryExecutor(IReadOnlyList<TestEntity> rows) : ICommandExecutor
    {
        public string? LastCommandText { get; private set; }

        public ValueTask<int> ExecuteAsync(
            string commandText,
            IReadOnlyList<SqlParameterValue>? parameters = null,
            CancellationToken cancellationToken = default)
        {
            LastCommandText = commandText;
            return ValueTask.FromResult(rows.Count);
        }

        public async IAsyncEnumerable<T> QueryAsync<T>(
            string commandText,
            Func<IDataRecord, T> materializer,
            IReadOnlyList<SqlParameterValue>? parameters = null,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            LastCommandText = commandText;
            foreach (var row in rows)
            {
                await Task.CompletedTask;
                yield return (T)(object)row;
            }
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
