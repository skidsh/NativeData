using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace NativeData.Abstractions;

/// <summary>
/// Convenience extension methods for <see cref="IRepository{T}"/>.
/// </summary>
public static class RepositoryExtensions
{
    /// <summary>
    /// Retrieves all entities as a <see cref="List{T}"/>.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="repository">Repository to query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of all entities in the table.</returns>
    public static async ValueTask<List<T>> GetAllToListAsync<T>(
        this IRepository<T> repository,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var list = new List<T>();
        await foreach (var item in repository.QueryAsync(cancellationToken: cancellationToken).WithCancellation(cancellationToken))
            list.Add(item);
        return list;
    }

    /// <summary>
    /// Queries entities matching an optional WHERE clause and returns them as a <see cref="List{T}"/>.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="repository">Repository to query.</param>
    /// <param name="whereClause">Optional SQL WHERE clause body (without the <c>WHERE</c> keyword).</param>
    /// <param name="parameters">Optional query parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of matching entities.</returns>
    public static async ValueTask<List<T>> QueryToListAsync<T>(
        this IRepository<T> repository,
        string? whereClause = null,
        IReadOnlyList<SqlParameterValue>? parameters = null,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var list = new List<T>();
        await foreach (var item in repository.QueryAsync(whereClause, parameters, cancellationToken).WithCancellation(cancellationToken))
            list.Add(item);
        return list;
    }

    /// <summary>
    /// Returns the first entity matching an optional WHERE clause, or <see langword="null"/> if none is found.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="repository">Repository to query.</param>
    /// <param name="whereClause">Optional SQL WHERE clause body (without the <c>WHERE</c> keyword).</param>
    /// <param name="parameters">Optional query parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The first matching entity, or <see langword="null"/>.</returns>
    public static async ValueTask<T?> FirstOrDefaultAsync<T>(
        this IRepository<T> repository,
        string? whereClause = null,
        IReadOnlyList<SqlParameterValue>? parameters = null,
        CancellationToken cancellationToken = default)
        where T : class
    {
        await foreach (var item in repository.QueryAsync(whereClause, parameters, cancellationToken).WithCancellation(cancellationToken))
            return item;
        return null;
    }

    /// <summary>
    /// Returns whether any entity exists matching an optional WHERE clause.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="repository">Repository to query.</param>
    /// <param name="whereClause">Optional SQL WHERE clause body (without the <c>WHERE</c> keyword).</param>
    /// <param name="parameters">Optional query parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see langword="true"/> if at least one matching entity exists; otherwise <see langword="false"/>.</returns>
    public static async ValueTask<bool> AnyAsync<T>(
        this IRepository<T> repository,
        string? whereClause = null,
        IReadOnlyList<SqlParameterValue>? parameters = null,
        CancellationToken cancellationToken = default)
        where T : class
    {
        await foreach (var _ in repository.QueryAsync(whereClause, parameters, cancellationToken).WithCancellation(cancellationToken))
            return true;
        return false;
    }

    /// <summary>
    /// Returns the number of entities matching an optional WHERE clause.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="repository">Repository to query.</param>
    /// <param name="whereClause">Optional SQL WHERE clause body (without the <c>WHERE</c> keyword).</param>
    /// <param name="parameters">Optional query parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The count of matching entities.</returns>
    public static async ValueTask<int> CountAsync<T>(
        this IRepository<T> repository,
        string? whereClause = null,
        IReadOnlyList<SqlParameterValue>? parameters = null,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var count = 0;
        await foreach (var _ in repository.QueryAsync(whereClause, parameters, cancellationToken).WithCancellation(cancellationToken))
            count++;
        return count;
    }
}
