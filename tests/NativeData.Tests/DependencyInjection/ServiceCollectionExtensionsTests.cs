using System.Data;
using Microsoft.Extensions.DependencyInjection;
using NativeData.Abstractions;
using NativeData.Core;
using NativeData.Extensions.DependencyInjection;
using NativeData.Sqlite;

namespace NativeData.Tests.DependencyInjection;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void RegistersContextAsScoped()
    {
        var services = new ServiceCollection();
        services.AddNativeData<TestContext>(o => o.UseSqlite("Data Source=:memory:"));

        var descriptor = services.Single(d => d.ServiceType == typeof(TestContext));
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void RegistersSingletonFactoryAndDialect()
    {
        var services = new ServiceCollection();
        services.AddNativeData<TestContext>(o => o.UseSqlite("Data Source=:memory:"));

        var factoryDescriptor = services.Single(d => d.ServiceType == typeof(IDbConnectionFactory));
        var dialectDescriptor = services.Single(d => d.ServiceType == typeof(ISqlDialect));

        Assert.Equal(ServiceLifetime.Singleton, factoryDescriptor.Lifetime);
        Assert.Equal(ServiceLifetime.Singleton, dialectDescriptor.Lifetime);
    }

    [Fact]
    public void ThrowsWhenNoProviderConfigured()
    {
        var services = new ServiceCollection();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            services.AddNativeData<TestContext>(_ => { }));

        Assert.Contains("No database provider configured", ex.Message);
    }

    [Fact]
    public async Task ResolvesContextWithRepositories()
    {
        var services = new ServiceCollection();
        services.AddNativeData<TestContext>(o => o.UseSqlite("Data Source=:memory:"));

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<TestContext>();
        Assert.NotNull(context);

        var repo = context.Items;
        Assert.NotNull(repo);
    }

    [Fact]
    public async Task ScopedContext_DisposedAtScopeEnd()
    {
        var services = new ServiceCollection();
        services.AddNativeData<TrackingContext>(o => o.UseSqlite("Data Source=:memory:"));

        await using var provider = services.BuildServiceProvider();

        TrackingContext contextRef;
        await using (var scope = provider.CreateAsyncScope())
        {
            contextRef = scope.ServiceProvider.GetRequiredService<TrackingContext>();
            Assert.False(contextRef.WasDisposed);
        }

        Assert.True(contextRef.WasDisposed);
    }

    [Fact]
    public async Task ScopedContext_IndependentPerScope()
    {
        var services = new ServiceCollection();
        services.AddNativeData<TestContext>(o => o.UseSqlite("Data Source=:memory:"));

        await using var provider = services.BuildServiceProvider();

        TestContext context1;
        TestContext context2;

        await using (var scope1 = provider.CreateAsyncScope())
        {
            context1 = scope1.ServiceProvider.GetRequiredService<TestContext>();
        }

        await using (var scope2 = provider.CreateAsyncScope())
        {
            context2 = scope2.ServiceProvider.GetRequiredService<TestContext>();
        }

        Assert.NotSame(context1, context2);
    }
}

public sealed record TestItem(int Id, string Name);

public sealed class TestItemMap : IEntityMap<TestItem>
{
    public string TableName => "Items";
    public string KeyColumn => "Id";
    public IReadOnlyList<string> WritableColumns => ["Id", "Name"];
    public object? GetKey(TestItem entity) => entity.Id;

    public IReadOnlyList<SqlParameterValue> BuildInsertParameters(TestItem entity)
        => [new("Id", entity.Id), new("Name", entity.Name)];

    public IReadOnlyList<SqlParameterValue> BuildUpdateParameters(TestItem entity)
        => [new("Id", entity.Id), new("Name", entity.Name)];

    public TestItem Materialize(IDataRecord record)
        => new(record.GetInt32(0), record.GetString(1));
}

public sealed class TestContext : NativeDataContext
{
    public TestContext(IDbConnectionFactory connectionFactory, ISqlDialect sqlDialect)
        : base(connectionFactory, sqlDialect)
    {
        RegisterMap(new TestItemMap());
    }

    public IRepository<TestItem> Items => Repository<TestItem>();
}

public sealed class TrackingContext : NativeDataContext
{
    public TrackingContext(IDbConnectionFactory connectionFactory, ISqlDialect sqlDialect)
        : base(connectionFactory, sqlDialect)
    {
        RegisterMap(new TestItemMap());
    }

    public bool WasDisposed { get; private set; }

    public override ValueTask DisposeAsync()
    {
        WasDisposed = true;
        return base.DisposeAsync();
    }
}
