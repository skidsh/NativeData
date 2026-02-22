using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace NativeData.Abstractions;

/// <summary>
/// Executes database commands and queries using provider-specific ADO.NET implementations.
/// </summary>
public interface ICommandExecutor
{
    /// <summary>
    /// Executes a non-query command.
    /// </summary>
    /// <param name="commandText">SQL command text to execute.</param>
    /// <param name="parameters">Optional command parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of affected rows reported by the provider.</returns>
    ValueTask<int> ExecuteAsync(
        string commandText,
        IReadOnlyList<SqlParameterValue>? parameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a query and materializes each record as <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Result item type.</typeparam>
    /// <param name="commandText">SQL query text to execute.</param>
    /// <param name="materializer">Materializer used to convert each data record to <typeparamref name="T"/>.</param>
    /// <param name="parameters">Optional query parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async stream of materialized results.</returns>
    IAsyncEnumerable<T> QueryAsync<T>(
        string commandText,
        Func<IDataRecord, T> materializer,
        IReadOnlyList<SqlParameterValue>? parameters = null,
        CancellationToken cancellationToken = default);
}