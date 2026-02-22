namespace NativeData.Abstractions;

public interface ISqlDialect
{
    string QuoteIdentifier(string identifier);

    string NormalizeParameterName(string parameterName);

    string BuildParameterName(string parameterName);
}