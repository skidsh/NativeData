using System.Data.Common;

namespace NativeData.Abstractions;

/// <summary>
/// Creates opened database connections for command execution.
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Opens and returns a database connection.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An opened <see cref="DbConnection"/> instance.</returns>
    ValueTask<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);
}