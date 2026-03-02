namespace NativeData.Abstractions;

/// <summary>
/// An AOT-safe ordering specification for a NativeData query.
/// </summary>
/// <param name="ColumnName">Unquoted column name to order by.</param>
/// <param name="Descending">When <see langword="true"/>, sorts in descending order.</param>
public readonly record struct QueryOrder(string ColumnName, bool Descending = false);
