namespace NativeData.Abstractions;

/// <summary>
/// Represents a named SQL parameter and its value.
/// </summary>
/// <param name="Name">Parameter name as expected by the command text.</param>
/// <param name="Value">Parameter value, or <see langword="null"/> to pass database null.</param>
public readonly record struct SqlParameterValue(string Name, object? Value);