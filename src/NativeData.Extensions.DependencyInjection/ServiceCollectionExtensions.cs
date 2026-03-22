using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using NativeData.Abstractions;
using NativeData.Core;

namespace NativeData.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering NativeData services in a <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers NativeData services including a scoped <typeparamref name="TContext"/>,
    /// singleton <see cref="IDbConnectionFactory"/>, and singleton <see cref="ISqlDialect"/>.
    /// </summary>
    /// <typeparam name="TContext">
    /// The <see cref="NativeDataContext"/> subclass to register as a scoped service.
    /// </typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">
    /// An action to configure <see cref="NativeDataOptions"/>. Use a provider extension
    /// such as <c>UseSqlite</c> or <c>UsePostgres</c> to set the connection factory and dialect.
    /// </param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the <paramref name="configure"/> action does not set a connection factory and dialect.
    /// </exception>
    public static IServiceCollection AddNativeData<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TContext>(
        this IServiceCollection services,
        Action<NativeDataOptions> configure)
        where TContext : NativeDataContext
    {
        var options = new NativeDataOptions();
        configure(options);

        if (options.ConnectionFactory is null || options.SqlDialect is null)
        {
            throw new InvalidOperationException(
                "No database provider configured. Call a provider method such as UseSqlite() or UsePostgres() " +
                "inside the AddNativeData configuration action.");
        }

        services.AddSingleton(options.ConnectionFactory);
        services.AddSingleton(options.SqlDialect);
        services.AddScoped<TContext>();

        return services;
    }
}
