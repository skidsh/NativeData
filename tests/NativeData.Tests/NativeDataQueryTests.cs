using System.Collections.Generic;
using System.Data;
using NativeData.Abstractions;
using NativeData.Core;

namespace NativeData.Tests;

public class NativeDataQueryTests
{
    // ── Where/OrderBy/Take/Skip SQL construction ──────────────────────────

    [Fact]
    public async Task ToListAsync_WithNoFilters_SelectsAllRows()
    {
        var executor = new CapturingExecutor([new TestEntity(1, "Alice"), new TestEntity(2, "Bob")]);
        var repo = new SqlRepository<TestEntity>(executor, new TestEntityMap());

        var result = await repo.Query().ToListAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("SELECT * FROM [TestEntities]", executor.LastCommandText);
    }

    [Fact]
    public async Task ToListAsync_WithWhereFilter_AppendsWhereClause()
    {
        var executor = new CapturingExecutor([new TestEntity(1, "Alice")]);
        var repo = new SqlRepository<TestEntity>(executor, new TestEntityMap());
        var filter = new QueryFilter("Name = @Name", [new SqlParameterValue("Name", "Alice")]);

        await repo.Query().Where(filter).ToListAsync();

        Assert.Contains("WHERE Name = @Name", executor.LastCommandText);
    }

    [Fact]
    public async Task ToListAsync_WithOrderBy_AppendsOrderByClause()
    {
        var executor = new CapturingExecutor([]);
        var repo = new SqlRepository<TestEntity>(executor, new TestEntityMap());

        await repo.Query().OrderBy(new QueryOrder("Name")).ToListAsync();

        Assert.Contains("ORDER BY [Name]", executor.LastCommandText);
    }

    [Fact]
    public async Task ToListAsync_WithOrderByDescending_AppendsDesc()
    {
        var executor = new CapturingExecutor([]);
        var repo = new SqlRepository<TestEntity>(executor, new TestEntityMap());

        await repo.Query().OrderBy(new QueryOrder("Name", Descending: true)).ToListAsync();

        Assert.Contains("ORDER BY [Name] DESC", executor.LastCommandText);
    }

    [Fact]
    public async Task ToListAsync_WithTake_AppendsLimitClause()
    {
        var executor = new CapturingExecutor([]);
        var repo = new SqlRepository<TestEntity>(executor, new TestEntityMap());

        await repo.Query().Take(5).ToListAsync();

        Assert.Contains("LIMIT 5", executor.LastCommandText);
    }

    [Fact]
    public async Task ToListAsync_WithSkip_AppendsOffsetClause()
    {
        var executor = new CapturingExecutor([]);
        var repo = new SqlRepository<TestEntity>(executor, new TestEntityMap());

        await repo.Query().Skip(10).ToListAsync();

        Assert.Contains("OFFSET 10", executor.LastCommandText);
    }

    [Fact]
    public async Task ToListAsync_WithTakeAndSkip_AppendsBothClauses()
    {
        var executor = new CapturingExecutor([]);
        var repo = new SqlRepository<TestEntity>(executor, new TestEntityMap());

        await repo.Query().Take(5).Skip(10).ToListAsync();

        Assert.Contains("LIMIT 5 OFFSET 10", executor.LastCommandText);
    }

    [Fact]
    public async Task ToListAsync_WithMultipleOrderBy_BuildsMultiColumnClause()
    {
        var executor = new CapturingExecutor([]);
        var repo = new SqlRepository<TestEntity>(executor, new TestEntityMap());

        await repo.Query()
            .OrderBy(new QueryOrder("Name"))
            .OrderBy(new QueryOrder("Id", Descending: true))
            .ToListAsync();

        Assert.Contains("ORDER BY [Name], [Id] DESC", executor.LastCommandText);
    }

    [Fact]
    public async Task FirstOrDefaultAsync_ReturnsFirstResult()
    {
        var executor = new CapturingExecutor([new TestEntity(1, "Alice"), new TestEntity(2, "Bob")]);
        var repo = new SqlRepository<TestEntity>(executor, new TestEntityMap());

        var result = await repo.Query().FirstOrDefaultAsync();

        Assert.NotNull(result);
        Assert.Equal("Alice", result!.Name);
    }

    [Fact]
    public async Task FirstOrDefaultAsync_ReturnsNull_WhenNoRows()
    {
        var executor = new CapturingExecutor([]);
        var repo = new SqlRepository<TestEntity>(executor, new TestEntityMap());

        var result = await repo.Query().FirstOrDefaultAsync();

        Assert.Null(result);
    }

    // ── NativeDataContext ─────────────────────────────────────────────────

    [Fact]
    public void NativeDataContext_Repository_ReturnsSameInstanceOnMultipleCalls()
    {
        var context = new TestContext(new NoOpConnectionFactory(), new DefaultSqlDialect());

        var repo1 = context.GetPeople();
        var repo2 = context.GetPeople();

        Assert.Same(repo1, repo2);
    }

    [Fact]
    public void NativeDataContext_Repository_Throws_WhenMapNotRegistered()
    {
        var context = new EmptyTestContext(new NoOpConnectionFactory(), new DefaultSqlDialect());

        var ex = Assert.Throws<InvalidOperationException>(() => context.GetPeople());

        Assert.Contains("TestEntity", ex.Message);
    }

    // ── Test infrastructure ───────────────────────────────────────────────

    private sealed class CapturingExecutor(IReadOnlyList<TestEntity> rows) : ICommandExecutor
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

    private sealed class NoOpConnectionFactory : IDbConnectionFactory
    {
        public ValueTask<System.Data.Common.DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
            => throw new NotSupportedException("NoOpConnectionFactory does not open real connections.");
    }

    private sealed class TestContext : NativeDataContext
    {
        public TestContext(IDbConnectionFactory factory, ISqlDialect dialect) : base(factory, dialect)
        {
            RegisterMap(new TestEntityMap());
        }

        public IRepository<TestEntity> GetPeople() => Repository<TestEntity>();
    }

    private sealed class EmptyTestContext(IDbConnectionFactory factory, ISqlDialect dialect)
        : NativeDataContext(factory, dialect)
    {
        public IRepository<TestEntity> GetPeople() => Repository<TestEntity>();
    }
}
