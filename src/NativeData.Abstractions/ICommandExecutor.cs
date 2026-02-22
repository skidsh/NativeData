using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace NativeData.Abstractions;

public interface ICommandExecutor
{
    ValueTask<int> ExecuteAsync(
        string commandText,
        IReadOnlyList<SqlParameterValue>? parameters = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<T> QueryAsync<T>(
        string commandText,
        Func<IDataRecord, T> materializer,
        IReadOnlyList<SqlParameterValue>? parameters = null,
        CancellationToken cancellationToken = default);
}