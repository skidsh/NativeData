using System.Collections.Generic;
using System.Data;

namespace NativeData.Abstractions;

/// <summary>
/// Defines table and materialization mapping for an entity type.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
public interface IEntityMap<T>
{
    /// <summary>
    /// Gets the mapped table name.
    /// </summary>
    string TableName { get; }

    /// <summary>
    /// Gets the mapped key column name.
    /// </summary>
    string KeyColumn { get; }

    /// <summary>
    /// Gets the writable column names.
    /// </summary>
    IReadOnlyList<string> WritableColumns { get; }

    /// <summary>
    /// Gets the key value from an entity instance.
    /// </summary>
    /// <param name="entity">Entity instance.</param>
    /// <returns>Key value.</returns>
    object? GetKey(T entity);

    /// <summary>
    /// Builds insert parameters for the entity.
    /// </summary>
    /// <param name="entity">Entity instance.</param>
    /// <returns>Parameters used for insert operations.</returns>
    IReadOnlyList<SqlParameterValue> BuildInsertParameters(T entity);

    /// <summary>
    /// Builds update parameters for the entity.
    /// </summary>
    /// <param name="entity">Entity instance.</param>
    /// <returns>Parameters used for update operations.</returns>
    IReadOnlyList<SqlParameterValue> BuildUpdateParameters(T entity);

    /// <summary>
    /// Materializes an entity from a data record.
    /// </summary>
    /// <param name="record">Source data record.</param>
    /// <returns>Materialized entity.</returns>
    T Materialize(IDataRecord record);
}