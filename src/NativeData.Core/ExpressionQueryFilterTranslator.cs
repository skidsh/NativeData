using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using NativeData.Abstractions;

namespace NativeData.Core;

internal static class ExpressionQueryFilterTranslator<T>
    where T : class
{
    public static QueryFilter Translate(
        Expression<Func<T, bool>> predicate,
        IEntityMap<T> entityMap,
        ISqlDialect sqlDialect)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(entityMap);
        ArgumentNullException.ThrowIfNull(sqlDialect);

        var allowedColumns = new HashSet<string>(entityMap.WritableColumns, StringComparer.OrdinalIgnoreCase)
        {
            entityMap.KeyColumn,
        };

        var parameters = new List<SqlParameterValue>();
        var parameterIndex = 0;
        var sql = TranslateNode(predicate.Body, predicate.Parameters[0], allowedColumns, sqlDialect, parameters, ref parameterIndex);
        return new QueryFilter(sql, parameters.Count == 0 ? null : parameters);
    }

    private static string TranslateNode(
        Expression expression,
        ParameterExpression parameter,
        HashSet<string> allowedColumns,
        ISqlDialect sqlDialect,
        List<SqlParameterValue> parameters,
        ref int parameterIndex)
    {
        expression = UnwrapConvert(expression);

        return expression switch
        {
            BinaryExpression binary when binary.NodeType is ExpressionType.AndAlso or ExpressionType.OrElse
                => TranslateLogical(binary, parameter, allowedColumns, sqlDialect, parameters, ref parameterIndex),
            BinaryExpression binary when IsComparison(binary.NodeType)
                => TranslateComparison(binary, parameter, allowedColumns, sqlDialect, parameters, ref parameterIndex),
            _ => throw NotSupported(expression, "Only comparison and boolean operator expressions are supported."),
        };
    }

    private static string TranslateLogical(
        BinaryExpression binary,
        ParameterExpression parameter,
        HashSet<string> allowedColumns,
        ISqlDialect sqlDialect,
        List<SqlParameterValue> parameters,
        ref int parameterIndex)
    {
        var op = binary.NodeType == ExpressionType.AndAlso ? "AND" : "OR";
        var left = TranslateNode(binary.Left, parameter, allowedColumns, sqlDialect, parameters, ref parameterIndex);
        var right = TranslateNode(binary.Right, parameter, allowedColumns, sqlDialect, parameters, ref parameterIndex);
        return $"({left} {op} {right})";
    }

    private static string TranslateComparison(
        BinaryExpression binary,
        ParameterExpression parameter,
        HashSet<string> allowedColumns,
        ISqlDialect sqlDialect,
        List<SqlParameterValue> parameters,
        ref int parameterIndex)
    {
        var left = UnwrapConvert(binary.Left);
        var right = UnwrapConvert(binary.Right);

        if (TryGetColumn(left, parameter, allowedColumns, out var column))
        {
            return BuildComparison(column, binary.NodeType, right, sqlDialect, parameters, ref parameterIndex);
        }

        if (TryGetColumn(right, parameter, allowedColumns, out column))
        {
            return BuildComparison(column, ReverseComparison(binary.NodeType), left, sqlDialect, parameters, ref parameterIndex);
        }

        throw NotSupported(binary, "One side of each comparison must reference an entity column.");
    }

    private static string BuildComparison(
        string column,
        ExpressionType comparisonType,
        Expression valueExpression,
        ISqlDialect sqlDialect,
        List<SqlParameterValue> parameters,
        ref int parameterIndex)
    {
        if (!TryEvaluateValue(valueExpression, out var value))
        {
            throw NotSupported(valueExpression, "Comparison value must be a constant or captured value.");
        }

        var quotedColumn = sqlDialect.QuoteIdentifier(column);

        if (value is null)
        {
            return comparisonType switch
            {
                ExpressionType.Equal => $"{quotedColumn} IS NULL",
                ExpressionType.NotEqual => $"{quotedColumn} IS NOT NULL",
                _ => throw NotSupported(valueExpression, "Null comparisons only support == and !=."),
            };
        }

        var parameterName = $"p{parameterIndex++}";
        var sqlParameterName = sqlDialect.BuildParameterName(parameterName);
        parameters.Add(new SqlParameterValue(parameterName, value));
        return $"{quotedColumn} {ToSqlComparison(comparisonType)} {sqlParameterName}";
    }

    private static bool TryGetColumn(
        Expression expression,
        ParameterExpression parameter,
        HashSet<string> allowedColumns,
        out string column)
    {
        column = string.Empty;
        expression = UnwrapConvert(expression);

        if (expression is not MemberExpression member || !IsParameterMember(member.Expression, parameter))
        {
            return false;
        }

        if (!allowedColumns.Contains(member.Member.Name))
        {
            throw NotSupported(expression, $"Member '{member.Member.Name}' is not a mapped column.");
        }

        column = member.Member.Name;
        return true;
    }

    private static bool IsParameterMember(Expression? expression, ParameterExpression parameter)
    {
        return expression is not null && UnwrapConvert(expression) == parameter;
    }

    private static bool TryEvaluateValue(Expression expression, out object? value)
    {
        expression = UnwrapConvert(expression);

        switch (expression)
        {
            case ConstantExpression constant:
                value = constant.Value;
                return true;
            case MemberExpression member:
                object? target;
                if (member.Expression is null)
                {
                    target = null;
                }
                else if (!TryEvaluateValue(member.Expression, out target))
                {
                    value = null;
                    return false;
                }

                switch (member.Member)
                {
                    case FieldInfo field:
                        value = field.GetValue(target);
                        return true;
                    case PropertyInfo property:
                        value = property.GetValue(target);
                        return true;
                    default:
                        value = null;
                        return false;
                }
            default:
                value = null;
                return false;
        }
    }

    private static Expression UnwrapConvert(Expression expression)
    {
        while (expression is UnaryExpression unary &&
               (unary.NodeType == ExpressionType.Convert || unary.NodeType == ExpressionType.ConvertChecked))
        {
            expression = unary.Operand;
        }

        return expression;
    }

    private static bool IsComparison(ExpressionType nodeType)
    {
        return nodeType is ExpressionType.Equal or
               ExpressionType.NotEqual or
               ExpressionType.GreaterThan or
               ExpressionType.GreaterThanOrEqual or
               ExpressionType.LessThan or
               ExpressionType.LessThanOrEqual;
    }

    private static ExpressionType ReverseComparison(ExpressionType nodeType)
    {
        return nodeType switch
        {
            ExpressionType.GreaterThan => ExpressionType.LessThan,
            ExpressionType.GreaterThanOrEqual => ExpressionType.LessThanOrEqual,
            ExpressionType.LessThan => ExpressionType.GreaterThan,
            ExpressionType.LessThanOrEqual => ExpressionType.GreaterThanOrEqual,
            _ => nodeType,
        };
    }

    private static string ToSqlComparison(ExpressionType nodeType)
    {
        return nodeType switch
        {
            ExpressionType.Equal => "=",
            ExpressionType.NotEqual => "<>",
            ExpressionType.GreaterThan => ">",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.LessThan => "<",
            ExpressionType.LessThanOrEqual => "<=",
            _ => throw new NotSupportedException($"Unsupported comparison operator '{nodeType}'."),
        };
    }

    private static NotSupportedException NotSupported(Expression expression, string reason)
    {
        return new NotSupportedException($"Unsupported predicate expression '{expression}': {reason}");
    }
}
