using System;

namespace NativeData.Abstractions;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class NativeDataEntityAttribute : Attribute
{
    public NativeDataEntityAttribute(string tableName, string keyColumn = "Id")
    {
        TableName = tableName;
        KeyColumn = keyColumn;
    }

    public string TableName { get; }

    public string KeyColumn { get; }
}