using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using NativeData.Abstractions;

namespace NativeData.Core;

public sealed class DbCommandExecutor : ICommandExecutor
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DbCommandExecutor(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async ValueTask<int> ExecuteAsync(
        string commandText,
        IReadOnlyList<SqlParameterValue>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        await using var command = BuildCommand(connection, commandText, parameters);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async IAsyncEnumerable<T> QueryAsync<T>(
        string commandText,
        Func<IDataRecord, T> materializer,
        IReadOnlyList<SqlParameterValue>? parameters = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        await using var command = BuildCommand(connection, commandText, parameters);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return materializer(reader);
        }
    }

    private static DbCommand BuildCommand(
        DbConnection connection,
        string commandText,
        IReadOnlyList<SqlParameterValue>? parameters)
    {
        var command = connection.CreateCommand();
        command.CommandText = commandText;

        if (parameters is null)
        {
            return command;
        }

        foreach (var parameter in parameters)
        {
            var dbParameter = command.CreateParameter();
            dbParameter.ParameterName = parameter.Name;
            dbParameter.Value = parameter.Value ?? DBNull.Value;
            command.Parameters.Add(dbParameter);
        }

        return command;
    }
}