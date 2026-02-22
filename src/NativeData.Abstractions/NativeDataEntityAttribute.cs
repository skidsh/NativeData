using System;

namespace NativeData.Abstractions;

/// <summary>
/// Marks an entity type for NativeData source-generation and mapping.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class NativeDataEntityAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NativeDataEntityAttribute"/> class.
    /// </summary>
    /// <param name="tableName">Mapped table name.</param>
    /// <param name="keyColumn">Mapped key column name.</param>
    public NativeDataEntityAttribute(string tableName, string keyColumn = "Id")
    {
        TableName = tableName;
        KeyColumn = keyColumn;
    }

    /// <summary>
    /// Gets the mapped table name.
    /// </summary>
    public string TableName { get; }

    /// <summary>
    /// Gets the mapped key column name.
    /// </summary>
    public string KeyColumn { get; }
}