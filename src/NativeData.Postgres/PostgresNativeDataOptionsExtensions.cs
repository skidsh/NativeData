using NativeData.Extensions.DependencyInjection;

namespace NativeData.Postgres;

/// <summary>
/// Extension methods for configuring NativeData to use PostgreSQL.
/// </summary>
public static class PostgresNativeDataOptionsExtensions
{
    /// <summary>
    /// Configures NativeData to use PostgreSQL with the specified connection string.
    /// </summary>
    /// <param name="options">The NativeData options.</param>
    /// <param name="connectionString">The Npgsql connection string.</param>
    /// <returns>The options instance for chaining.</returns>
    public static NativeDataOptions UsePostgres(this NativeDataOptions options, string connectionString)
    {
        options.UseConnectionFactory(
            new PostgresConnectionFactory(connectionString),
            new PostgresSqlDialect());
        return options;
    }
}
