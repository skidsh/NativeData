using NativeData.Extensions.DependencyInjection;

namespace NativeData.Sqlite;

/// <summary>
/// Extension methods for configuring NativeData to use SQLite.
/// </summary>
public static class SqliteNativeDataOptionsExtensions
{
    /// <summary>
    /// Configures NativeData to use SQLite with the specified connection string.
    /// </summary>
    /// <param name="options">The NativeData options.</param>
    /// <param name="connectionString">The SQLite connection string.</param>
    /// <returns>The options instance for chaining.</returns>
    public static NativeDataOptions UseSqlite(this NativeDataOptions options, string connectionString)
    {
        options.UseConnectionFactory(
            new SqliteConnectionFactory(connectionString),
            new SqliteSqlDialect());
        return options;
    }
}
