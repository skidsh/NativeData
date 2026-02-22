namespace NativeData.Abstractions;

public readonly record struct SqlParameterValue(string Name, object? Value);