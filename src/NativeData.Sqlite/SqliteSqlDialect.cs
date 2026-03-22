using NativeData.Abstractions;

namespace NativeData.Sqlite;

/// <summary>
/// SQL dialect for SQLite. Uses double-quoted identifiers and <c>@</c>-prefixed parameters,
/// compatible with Microsoft.Data.Sqlite's default parameter binding behavior.
/// </summary>
public sealed class SqliteSqlDialect : ISqlDialect
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