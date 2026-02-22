namespace NativeData.Abstractions;

/// <summary>
/// Defines SQL syntax customization points for a provider dialect.
/// </summary>
public interface ISqlDialect
{
    /// <summary>
    /// Quotes a table or column identifier.
    /// </summary>
    /// <param name="identifier">Unquoted identifier.</param>
    /// <returns>Quoted identifier for the current dialect.</returns>
    string QuoteIdentifier(string identifier);

    /// <summary>
    /// Normalizes a parameter name by removing provider prefix characters.
    /// </summary>
    /// <param name="parameterName">Parameter name to normalize.</param>
    /// <returns>Normalized parameter name without prefix characters.</returns>
    string NormalizeParameterName(string parameterName);

    /// <summary>
    /// Builds a dialect-specific parameter name.
    /// </summary>
    /// <param name="parameterName">Base parameter name.</param>
    /// <returns>Dialect-formatted parameter name.</returns>
    string BuildParameterName(string parameterName);
}