using System.Collections.Generic;
using System.Data;

namespace NativeData.Abstractions;

public interface IEntityMap<T>
{
    string TableName { get; }

    string KeyColumn { get; }

    IReadOnlyList<string> WritableColumns { get; }

    object? GetKey(T entity);

    IReadOnlyList<SqlParameterValue> BuildInsertParameters(T entity);

    IReadOnlyList<SqlParameterValue> BuildUpdateParameters(T entity);

    T Materialize(IDataRecord record);
}