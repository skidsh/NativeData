using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NativeData.Abstractions;

/// <summary>
/// Provides basic CRUD operations for an entity type.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
public interface IRepository<T>
    where T : class
{
    /// <summary>
    /// Retrieves an entity by key.
    /// </summary>
    /// <param name="id">Entity key value.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The entity when found; otherwise <see langword="null"/>.</returns>
    ValueTask<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries entities with an optional raw WHERE clause.
    /// </summary>
    /// <param name="whereClause">Optional SQL WHERE clause body (without the <c>WHERE</c> keyword).</param>
    /// <param name="parameters">Optional query parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async stream of matching entities.</returns>
    IAsyncEnumerable<T> QueryAsync(
        string? whereClause = null,
        IReadOnlyList<SqlParameterValue>? parameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts an entity.
    /// </summary>
    /// <param name="entity">Entity to insert.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of affected rows reported by the provider.</returns>
    ValueTask<int> InsertAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an entity.
    /// </summary>
    /// <param name="entity">Entity to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of affected rows reported by the provider.</returns>
    ValueTask<int> UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity by key.
    /// </summary>
    /// <param name="id">Entity key value.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of affected rows reported by the provider.</returns>
    ValueTask<int> DeleteByIdAsync(object id, CancellationToken cancellationToken = default);
}