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

    /// <summary>
    /// Builds a LIMIT/OFFSET clause fragment for this dialect.
    /// Returns an empty string when both <paramref name="take"/> and <paramref name="skip"/> are <see langword="null"/>.
    /// </summary>
    /// <param name="take">Maximum number of rows to return, or <see langword="null"/> for no limit.</param>
    /// <param name="skip">Number of rows to skip before returning results, or <see langword="null"/> for no offset.</param>
    /// <returns>A SQL clause fragment such as <c> LIMIT 10 OFFSET 20</c>, or empty string.</returns>
    string BuildLimitOffsetClause(int? take, int? skip) =>
        (take, skip) switch
        {
            (null, null) => string.Empty,
            (not null, null) => $" LIMIT {take.Value}",
            (null, not null) => $" OFFSET {skip.Value}",
            _ => $" LIMIT {take!.Value} OFFSET {skip!.Value}",
        };
}