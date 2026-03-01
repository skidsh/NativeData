using NativeData.Abstractions;
using Npgsql;
using System.Data.Common;

namespace NativeData.Postgres;

/// <summary>
/// <see cref="IDbConnectionFactory"/> implementation for PostgreSQL using Npgsql.
/// </summary>
public sealed class PostgresConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgresConnectionFactory"/> class.
    /// </summary>
    /// <param name="connectionString">Npgsql connection string.</param>
    public PostgresConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <inheritdoc />
    public async ValueTask<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
