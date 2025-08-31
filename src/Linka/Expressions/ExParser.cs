using System.Linq.Expressions;
using System.Reflection;
using Webamoki.Linka.Fields;
using Webamoki.Linka.ModelSystem;

namespace Webamoki.Linka.Expressions;

/// <summary>
/// This class is used to build SQL queries for given Model Classes.
/// Contains methods to create Select queries and to parse conditions.
/// </summary>
internal static class ExParser
{
    public static IEx<T>? Parse<T>(Expression<Func<T, bool>> expr, out string? error) where T : Model
    {
        try
        {
            error = null;
            return ParseBinaryExpression<T>((BinaryExpression)expr.Body);
        }
        catch(FormatException e)
        {
            error = e.Message;
            return null;
        }
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
            var isAnd = op switch
            {
                "AND" => true,
                "OR" => false,
                _ => throw new NotSupportedException($"Unsupported operator for binary expression: {op}")
            };
            var left = ParseBinaryExpression<T>(bExpr);
            var right = ParseBinaryExpression<T>((BinaryExpression)expr.Right);
            return new Ex<T>(left, isAnd,right);
        }

        throw new NotSupportedException($"Unsupported expression: {expr.Left}");
    }

    public static ConditionEx<T> ParseCondition<T>(
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

        return field.ParseEx<T>(op, value);
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