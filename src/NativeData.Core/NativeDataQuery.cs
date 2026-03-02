using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NativeData.Abstractions;

namespace NativeData.Core;

/// <summary>
/// A fluent, AOT-safe query builder for a single entity type.
/// Construct via <see cref="SqlRepository{T}.Query"/>.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
public sealed class NativeDataQuery<T>
    where T : class
{
    private readonly ICommandExecutor _commandExecutor;
    private readonly IEntityMap<T> _entityMap;
    private readonly ISqlDialect _sqlDialect;

    private QueryFilter? _where;
    private readonly List<QueryOrder> _orderBy = [];
    private int? _take;
    private int? _skip;

    internal NativeDataQuery(ICommandExecutor commandExecutor, IEntityMap<T> entityMap, ISqlDialect sqlDialect)
    {
        _commandExecutor = commandExecutor;
        _entityMap = entityMap;
        _sqlDialect = sqlDialect;
    }

    /// <summary>
    /// Filters results to rows matching the given predicate.
    /// Replaces any previously set filter.
    /// </summary>
    /// <param name="filter">AOT-safe predicate containing a SQL WHERE clause and parameters.</param>
    /// <returns>This query builder for chaining.</returns>
    public NativeDataQuery<T> Where(QueryFilter filter)
    {
        _where = filter;
        return this;
    }

    /// <summary>
    /// Appends an ORDER BY column to the query.
    /// Multiple calls produce a multi-column ORDER BY clause.
    /// </summary>
    /// <param name="order">Column and direction to order by.</param>
    /// <returns>This query builder for chaining.</returns>
    public NativeDataQuery<T> OrderBy(QueryOrder order)
    {
        _orderBy.Add(order);
        return this;
    }

    /// <summary>
    /// Limits the number of rows returned.
    /// </summary>
    /// <param name="count">Maximum number of rows.</param>
    /// <returns>This query builder for chaining.</returns>
    public NativeDataQuery<T> Take(int count)
    {
        _take = count;
        return this;
    }

    /// <summary>
    /// Skips the specified number of rows before returning results.
    /// </summary>
    /// <param name="count">Number of rows to skip.</param>
    /// <returns>This query builder for chaining.</returns>
    public NativeDataQuery<T> Skip(int count)
    {
        _skip = count;
        return this;
    }

    /// <summary>
    /// Executes the query and returns an async stream of results.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async stream of matching entities.</returns>
    public IAsyncEnumerable<T> AsAsyncEnumerable(CancellationToken cancellationToken = default)
    {
        var (sql, parameters) = BuildQuery();
        return _commandExecutor.QueryAsync(sql, _entityMap.Materialize, parameters, cancellationToken);
    }

    /// <summary>
    /// Executes the query and collects all results into a <see cref="List{T}"/>.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of all matching entities.</returns>
    public async ValueTask<List<T>> ToListAsync(CancellationToken cancellationToken = default)
    {
        var list = new List<T>();
        await foreach (var item in AsAsyncEnumerable(cancellationToken).WithCancellation(cancellationToken))
            list.Add(item);
        return list;
    }

    /// <summary>
    /// Executes the query and returns the first matching entity, or <see langword="null"/> if none is found.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The first matching entity, or <see langword="null"/>.</returns>
    public async ValueTask<T?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        await foreach (var item in AsAsyncEnumerable(cancellationToken).WithCancellation(cancellationToken))
            return item;
        return null;
    }

    internal (string Sql, IReadOnlyList<SqlParameterValue>? Parameters) BuildQuery()
    {
        var tableName = _sqlDialect.QuoteIdentifier(_entityMap.TableName);
        var sb = new StringBuilder("SELECT * FROM ").Append(tableName);

        IReadOnlyList<SqlParameterValue>? parameters = null;

        if (_where.HasValue)
        {
            sb.Append(" WHERE ").Append(_where.Value.Sql);
            parameters = _where.Value.Parameters;
        }

        if (_orderBy.Count > 0)
        {
            sb.Append(" ORDER BY ");
            for (var i = 0; i < _orderBy.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                var order = _orderBy[i];
                sb.Append(_sqlDialect.QuoteIdentifier(order.ColumnName));
                if (order.Descending) sb.Append(" DESC");
            }
        }

        var limitOffset = _sqlDialect.BuildLimitOffsetClause(_take, _skip);
        if (limitOffset.Length > 0) sb.Append(limitOffset);

        return (sb.ToString(), parameters);
    }
}
