using Microsoft.Data.Sqlite;
using NativeData.Abstractions;
using System.Data.Common;

namespace NativeData.Sqlite;

/// <summary>
/// <see cref="IDbConnectionFactory"/> implementation for SQLite using Microsoft.Data.Sqlite.
/// </summary>
public sealed class SqliteConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteConnectionFactory"/> class.
    /// </summary>
    /// <param name="connectionString">SQLite connection string.</param>
    public SqliteConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <inheritdoc />
    public async ValueTask<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}