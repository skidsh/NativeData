using System.Collections.Generic;
using NativeData.Abstractions;

namespace NativeData.Core;

public sealed class SqlRepository<T> : IRepository<T>
    where T : class
{
    private readonly ICommandExecutor _commandExecutor;
    private readonly IEntityMap<T> _entityMap;
    private readonly ISqlDialect _sqlDialect;

    public SqlRepository(
        ICommandExecutor commandExecutor,
        IEntityMap<T> entityMap,
        ISqlDialect? sqlDialect = null)
    {
        _commandExecutor = commandExecutor;
        _entityMap = entityMap;
        _sqlDialect = sqlDialect ?? new DefaultSqlDialect();
    }

    public async ValueTask<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        var tableName = Quote(_entityMap.TableName);
        var keyColumn = Quote(_entityMap.KeyColumn);
        var keyParameter = _sqlDialect.BuildParameterName(_entityMap.KeyColumn);
        var query = $"SELECT * FROM {tableName} WHERE {keyColumn} = {keyParameter}";

        await foreach (var item in _commandExecutor.QueryAsync(
                           query,
                           _entityMap.Materialize,
                           [new SqlParameterValue(keyParameter, id)],
                           cancellationToken))
        {
            return item;
        }

        return null;
    }

    public IAsyncEnumerable<T> QueryAsync(
        string? whereClause = null,
        IReadOnlyList<SqlParameterValue>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var tableName = Quote(_entityMap.TableName);
        var query = $"SELECT * FROM {tableName}";

        if (!string.IsNullOrWhiteSpace(whereClause))
        {
            query = $"{query} WHERE {whereClause}";
        }

        return _commandExecutor.QueryAsync(query, _entityMap.Materialize, parameters, cancellationToken);
    }

    public ValueTask<int> InsertAsync(T entity, CancellationToken cancellationToken = default)
    {
        var tableName = Quote(_entityMap.TableName);
        var parameters = _entityMap.BuildInsertParameters(entity);
        var command = BuildInsertCommand(tableName, parameters);

        return _commandExecutor.ExecuteAsync(command, parameters, cancellationToken);
    }

    public ValueTask<int> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        var tableName = Quote(_entityMap.TableName);
        var parameters = _entityMap.BuildUpdateParameters(entity);
        var command = BuildUpdateCommand(tableName, parameters);

        return _commandExecutor.ExecuteAsync(command, parameters, cancellationToken);
    }

    public ValueTask<int> DeleteByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        var tableName = Quote(_entityMap.TableName);
        var keyColumn = Quote(_entityMap.KeyColumn);
        var keyParameter = _sqlDialect.BuildParameterName(_entityMap.KeyColumn);
        var command = $"DELETE FROM {tableName} WHERE {keyColumn} = {keyParameter}";

        return _commandExecutor.ExecuteAsync(
            command,
            [new SqlParameterValue(keyParameter, id)],
            cancellationToken);
    }

    private string BuildInsertCommand(string tableName, IReadOnlyList<SqlParameterValue> parameters)
    {
        var columnNames = parameters.Select(p => Quote(_sqlDialect.NormalizeParameterName(p.Name))).ToArray();
        var parameterNames = parameters.Select(p => _sqlDialect.BuildParameterName(p.Name)).ToArray();

        return $"INSERT INTO {tableName} ({string.Join(", ", columnNames)}) VALUES ({string.Join(", ", parameterNames)})";
    }

    private string BuildUpdateCommand(string tableName, IReadOnlyList<SqlParameterValue> parameters)
    {
        var keyName = _sqlDialect.NormalizeParameterName(_entityMap.KeyColumn);
        var assignments = new List<string>();

        foreach (var parameter in parameters)
        {
            var parameterName = _sqlDialect.NormalizeParameterName(parameter.Name);
            if (parameterName.Equals(keyName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            assignments.Add($"{Quote(parameterName)} = {_sqlDialect.BuildParameterName(parameterName)}");
        }

        var keyColumn = Quote(_entityMap.KeyColumn);
        var keyParameter = _sqlDialect.BuildParameterName(_entityMap.KeyColumn);
        return $"UPDATE {tableName} SET {string.Join(", ", assignments)} WHERE {keyColumn} = {keyParameter}";
    }

    private string Quote(string identifier)
    {
        return _sqlDialect.QuoteIdentifier(identifier);
    }
}