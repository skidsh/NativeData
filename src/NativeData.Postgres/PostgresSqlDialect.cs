using NativeData.Abstractions;

namespace NativeData.Postgres;

/// <summary>
/// SQL dialect for PostgreSQL. Uses double-quoted identifiers and <c>@</c>-prefixed parameters,
/// compatible with Npgsql's default parameter binding behavior.
/// </summary>
public sealed class PostgresSqlDialect : ISqlDialect
{
    /// <inheritdoc />
    public string QuoteIdentifier(string identifier)
    {
        return $"\"{identifier}\"";
    }

    /// <inheritdoc />
    public string NormalizeParameterName(string parameterName)
    {
        return parameterName.TrimStart('@', ':', '$');
    }

    /// <inheritdoc />
    public string BuildParameterName(string parameterName)
    {
        return $"@{NormalizeParameterName(parameterName)}";
    }
}
