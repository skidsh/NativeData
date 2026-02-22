using NativeData.Abstractions;

namespace NativeData.Core;

/// <summary>
/// Default SQL dialect using bracket-quoted identifiers and <c>@</c>-prefixed parameters.
/// </summary>
public sealed class DefaultSqlDialect : ISqlDialect
{
    /// <inheritdoc />
    public string QuoteIdentifier(string identifier)
    {
        return $"[{identifier}]";
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