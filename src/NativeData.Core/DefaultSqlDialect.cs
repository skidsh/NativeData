using NativeData.Abstractions;

namespace NativeData.Core;

public sealed class DefaultSqlDialect : ISqlDialect
{
    public string QuoteIdentifier(string identifier)
    {
        return $"[{identifier}]";
    }

    public string NormalizeParameterName(string parameterName)
    {
        return parameterName.TrimStart('@', ':', '$');
    }

    public string BuildParameterName(string parameterName)
    {
        return $"@{NormalizeParameterName(parameterName)}";
    }
}