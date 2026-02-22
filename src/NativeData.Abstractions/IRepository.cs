using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NativeData.Abstractions;

public interface IRepository<T>
    where T : class
{
    ValueTask<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default);

    IAsyncEnumerable<T> QueryAsync(
        string? whereClause = null,
        IReadOnlyList<SqlParameterValue>? parameters = null,
        CancellationToken cancellationToken = default);

    ValueTask<int> InsertAsync(T entity, CancellationToken cancellationToken = default);

    ValueTask<int> UpdateAsync(T entity, CancellationToken cancellationToken = default);

    ValueTask<int> DeleteByIdAsync(object id, CancellationToken cancellationToken = default);
}