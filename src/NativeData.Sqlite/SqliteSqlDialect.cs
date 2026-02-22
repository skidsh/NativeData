using NativeData.Abstractions;

namespace NativeData.Sqlite;

public sealed class SqliteSqlDialect : ISqlDialect
{
    public string QuoteIdentifier(string identifier)
    {
        return $"\"{identifier}\"";
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