using NativeData.Abstractions;

namespace NativeData.Extensions.DependencyInjection;

/// <summary>
/// Configuration options for NativeData DI registration.
/// Provider extensions (e.g., <c>UseSqlite</c>, <c>UsePostgres</c>) call
/// <see cref="UseConnectionFactory"/> to supply the connection factory and dialect.
/// </summary>
public sealed class NativeDataOptions
{
    /// <summary>
    /// Gets the configured connection factory, or <see langword="null"/> if no provider has been configured.
    /// </summary>
    public IDbConnectionFactory? ConnectionFactory { get; internal set; }

    /// <summary>
    /// Gets the configured SQL dialect, or <see langword="null"/> if no provider has been configured.
    /// </summary>
    public ISqlDialect? SqlDialect { get; internal set; }

    /// <summary>
    /// Sets the connection factory and SQL dialect. Called by provider-specific extension methods.
    /// </summary>
    /// <param name="connectionFactory">The database connection factory.</param>
    /// <param name="sqlDialect">The SQL dialect for the provider.</param>
    public void UseConnectionFactory(IDbConnectionFactory connectionFactory, ISqlDialect sqlDialect)
    {
        ConnectionFactory = connectionFactory;
        SqlDialect = sqlDialect;
    }
}
