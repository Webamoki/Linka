using System.Linq.Expressions;
using System.Reflection;
using Webamoki.Linka.Expressions.Ex;
using Webamoki.Linka.Fields;
using Webamoki.Linka.ModelSystem;
using Webamoki.Linka.Queries;

namespace Webamoki.Linka.Expressions;

/// <summary>
/// This class is used to build SQL queries for given Model Classes.
/// Contains methods to create Select queries and to parse conditions.
/// </summary>
internal static class ExCompiler
{
    public static SelectQuery GetQuery<T>() where T : Model
    {
        var query = new SelectQuery();
        query.AddTable<T>();
        var modelInfo = ModelRegistry.Get<T>();
        foreach (var field in modelInfo.Fields.Values)
        {
            query.Select<T>(field);
        }

        return query;
    }

    public static IEx<T> Parse<T> (Expression<Func<T, bool>> expr) where T : Model
    {
        return ParseBinaryExpression<T>((BinaryExpression)expr.Body);
        // if (query.StartsWith('(') && query.EndsWith(')')) query = query[1..^1];
    }
    
    private static IEx<T> ParseBinaryExpression<T>(BinaryExpression expr) where T : Model
    {
        var op = GetOperator(expr.NodeType);
        if (expr.Left is MemberExpression fieldExpr)
        {
            var fieldName = fieldExpr.Member.Name;
            if (!ModelRegistry.Get<T>().Fields.TryGetValue(fieldName, out var field))
                throw new NotSupportedException($"Field {fieldName} not found in model {typeof(T).Name}");
            var value = ParseValueExpression(expr.Right);
            if (!field.IsValid(value, out var message))
            {
                throw new FormatException($"Invalid value for field {fieldName}: {message}");
            }
            return ParseCondition<T>(fieldName, field, op, value);
        }

        if (expr.Left is BinaryExpression bExpr)
        {
            var left = ParseBinaryExpression<T>(bExpr);
            var right = ParseBinaryExpression<T>((BinaryExpression)expr.Right);
            return new Ex<T>(left, op,right);
        }

        throw new NotSupportedException($"Unsupported expression: {expr.Left}");
    }

    public static IConditionEx<T> ParseCondition<T>(
        string name,
        DbField field,
        string op,
        object? value
        ) where T : Model
    {
        if (value is null)
        {
            if (field.IsRequired)
                throw new FormatException($"Field {name} is required and cannot be null.");
            return op switch
            {
                "=" => new NullEx<T>(name, false),
                "!=" => new NullEx<T>(name, true),
                _ => throw new NotSupportedException($"Unsupported operator for null value: {op}")
            };
        }
        
        
        if (field.Validator.IsInjectable)
        {
            values = [ToStringValue(value)];
            return $"{fieldName} {op} ?";
        }

        if (value is string or Enum) return $"{fieldName} {op} '{ToStringValue(value)}'";
        return $"{fieldName} {op} {ToStringValue(value)}";
    }
    
    
    
    public static string Condition<T>(Expression<Func<T, bool>> expr,out List<object> values, out string? error) where T : Model
    {
        try
        {
            error = null;
            var table = Model.TableName<T>();
            var query = ParseBinaryExpression<T>((BinaryExpression)expr.Body,table,out values);
            if (query.StartsWith('(') && query.EndsWith(')')) return query[1..^1];
            return query;
        }
        catch(FormatException e)
        {
            error = e.Message;
            values = [];
            return "";
        }
    }

    private static string ToStringValue(object value)
    {
        return value switch
        {
            string s => s,
            int i => i.ToString(),
            Enum e => e.ToString(),
            bool b => b ? "true" : "false",
            _ => throw new NotSupportedException($"Unsupported expression: {value.GetType().Name}")
        };
    }

    private static string GetOperator(ExpressionType nodeType)
    {
        return nodeType switch
        {
            ExpressionType.Equal => "=",
            ExpressionType.NotEqual => "!=",
            ExpressionType.GreaterThan => ">",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.LessThan => "<",
            ExpressionType.LessThanOrEqual => "<=",
            ExpressionType.AndAlso => "AND",
            ExpressionType.OrElse => "OR",
            _ => throw new NotSupportedException($"Unsupported operator: {nodeType}")
        };
    }
    private static string ParseBinaryExpression<T>(BinaryExpression expr,string table, out List<object> values) where T : Model
    {
        var op = GetOperator(expr.NodeType);
        if (expr.Left is MemberExpression fieldExpr)
        {
            var fieldName = fieldExpr.Member.Name;
            if (!ModelRegistry.Get<T>().Fields.TryGetValue(fieldName, out var field))
                throw new NotSupportedException($"Field {fieldName} not found in model {typeof(T).Name}");
            var value = ParseValueExpression(expr.Right);
            if (!field.IsValid(value, out var message))
            {
                throw new FormatException($"Invalid value for field {fieldName}: {message}");
            }
            fieldName = $"\"{table}\".\"{fieldName}\"";
            if (value is null)
            {
                op = op switch
                {
                    "=" => "IS",
                    "!=" => "IS NOT",
                    _ => throw new NotSupportedException($"Unsupported operator for null value: {op}")
                };
                values = [];
                return $"{fieldName} {op} NULL";
            }
            values = [];
            if (field.Validator.IsInjectable)
            {
                values = [ToStringValue(value)];
                return $"{fieldName} {op} ?";
            }

            values = [];
            if (value is string or Enum) return $"{fieldName} {op} '{ToStringValue(value)}'";
            return $"{fieldName} {op} {ToStringValue(value)}";
        }

        if (expr.Left is BinaryExpression bExpr)
        {
            var left = ParseBinaryExpression<T>(bExpr,table, out values);
            var right = ParseBinaryExpression<T>((BinaryExpression)expr.Right,table, out var rValues);
            values.AddRange(rValues);
            return $"({left} {op} {right})";
        }

        throw new NotSupportedException($"Unsupported expression: {expr.Left}");
    }
    
    private static object ParseValueExpression(Expression expr)
    {
        switch (expr)
        {
            case MemberExpression member:
                if (member.Expression is not ConstantExpression closure)
                    throw new NotSupportedException("Invalid Value Expression");
                var container = closure.Value!;
                var field = member.Member as FieldInfo;
                return field!.GetValue(container)!;
            case UnaryExpression unary:
                if (unary.NodeType != ExpressionType.Convert)
                    throw new NotSupportedException("Invalid Value Expression");
                return ParseValueExpression(unary.Operand);
            case ConstantExpression constant:
                return constant.Value!;
        }
        throw new NotSupportedException("Invalid Value Expression");
    }
}