using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NativeData.Abstractions;

namespace NativeData.Core;

/// <summary>
/// Abstract base class for a scoped NativeData database context.
/// Subclass this to define typed repository accessors and register entity maps
/// using <see cref="RegisterMap{T}"/> in the constructor.
/// </summary>
/// <remarks>
/// <para>
/// NativeDataContext is designed for scoped lifetime semantics (e.g., per-request in ASP.NET Core).
/// Repository instances are cached for the lifetime of the context and disposed with it.
/// </para>
/// <para>
/// Entity maps must be registered without reflection using the generated
/// <c>NativeDataEntityMaps.Create&lt;T&gt;()</c> factory or a manually constructed <see cref="IEntityMap{T}"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public sealed class AppContext : NativeDataContext
/// {
///     public AppContext(IDbConnectionFactory factory, ISqlDialect dialect)
///         : base(factory, dialect)
///     {
///         RegisterMap(NativeDataEntityMaps.Create&lt;Person&gt;());
///         RegisterMap(NativeDataEntityMaps.Create&lt;Order&gt;());
///     }
///
///     public IRepository&lt;Person&gt; People =&gt; Repository&lt;Person&gt;();
///     public IRepository&lt;Order&gt; Orders =&gt; Repository&lt;Order&gt;();
/// }
/// </code>
/// </example>
public abstract class NativeDataContext : IAsyncDisposable
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ISqlDialect _sqlDialect;
    private readonly Dictionary<Type, object> _entityMaps = [];
    private readonly Dictionary<Type, object> _repositoryCache = [];

    /// <summary>
    /// Initializes a new instance of <see cref="NativeDataContext"/>.
    /// </summary>
    /// <param name="connectionFactory">Factory used to open database connections.</param>
    /// <param name="sqlDialect">SQL dialect for identifier quoting and parameter naming.</param>
    protected NativeDataContext(IDbConnectionFactory connectionFactory, ISqlDialect sqlDialect)
    {
        _connectionFactory = connectionFactory;
        _sqlDialect = sqlDialect;
    }

    /// <summary>
    /// Registers an entity map for type <typeparamref name="T"/>.
    /// Call this in your subclass constructor, typically using the generated
    /// <c>NativeDataEntityMaps.Create&lt;T&gt;()</c> factory.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="entityMap">Map that describes the entity's table, key, and column mappings.</param>
    protected void RegisterMap<T>(IEntityMap<T> entityMap)
        where T : class
        => _entityMaps[typeof(T)] = entityMap;

    /// <summary>
    /// Returns a cached <see cref="IRepository{T}"/> for type <typeparamref name="T"/>.
    /// The repository is created on first access and reused for the lifetime of this context.
    /// </summary>
    /// <typeparam name="T">Entity type. An entity map must have been registered via <see cref="RegisterMap{T}"/>.</typeparam>
    /// <returns>A <see cref="SqlRepository{T}"/> backed by this context's connection factory and dialect.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no entity map has been registered for <typeparamref name="T"/>.
    /// </exception>
    protected IRepository<T> Repository<T>()
        where T : class
    {
        if (!_repositoryCache.TryGetValue(typeof(T), out var cached))
        {
            if (!_entityMaps.TryGetValue(typeof(T), out var mapObj))
            {
                throw new InvalidOperationException(
                    $"No entity map is registered for '{typeof(T).Name}'. " +
                    $"Call RegisterMap<{typeof(T).Name}>() in the context constructor.");
            }

            var map = (IEntityMap<T>)mapObj;
            var executor = new DbCommandExecutor(_connectionFactory);
            cached = new SqlRepository<T>(executor, map, _sqlDialect);
            _repositoryCache[typeof(T)] = cached;
        }

        return (IRepository<T>)cached;
    }

    /// <summary>
    /// Disposes the context and clears the repository cache.
    /// </summary>
    /// <returns>A completed <see cref="ValueTask"/>.</returns>
    public virtual ValueTask DisposeAsync()
    {
        _repositoryCache.Clear();
        _entityMaps.Clear();
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}
