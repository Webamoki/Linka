using Webamoki.Linka.ModelSystem;

namespace Webamoki.Linka.Expressions.Ex;

internal interface IEx<T> where T : Model
{
    public string ToQuery(out List<object> values);
}
internal record Ex<T>(IEx<T> left, bool isAND, IEx<T> right) :  IEx<T> where T : Model
{
    public string ToQuery(out List<object> values)
    {
        var leftQuery = left.ToQuery(out var leftValues);
        var rightQuery = right.ToQuery(out var rightValues);
        values = leftValues;
        values.AddRange(rightValues);
        var op = isAND ? "AND" : "OR";
        return $"({leftQuery} {op} {rightQuery})";
    }
}
internal interface IConditionEx<T>: IEx<T> where T : Model;

internal record StringEx<T>(string name, bool isEqual, object value, bool isInjectable) : IConditionEx<T> where T : Model
{
    public string ToQuery(out List<object> values)
    {
        var op = isEqual ? "=" : "!=";
        values = [];
        if (isInjectable)
        {
            values.Add(value);
            return $"{name} {op} ?";
        }

        if (value is string strValue)
        {
            return $"{name} {op} '{strValue.Replace("'", "''")}'";
        }
        else if (value is DateTime dtValue)
        {
            return $"{name} {op} '{dtValue:yyyy-MM-dd HH:mm:ss}'";
        }
        else if (value is bool bValue)
        {
            return $"{name} {op} {(bValue ? 1 : 0)}";
        }
        else
        {
            return $"{name} {op} {value}";
        }
    }
}
internal record NullEx<T>(string name, bool isEqual) : IConditionEx<T> where T : Model;
internal record EnumEx<T>(string name, bool isEqual, string value) : IConditionEx<T> where T : Model;
internal record IntEx<T>(string name, string op, int value) : IConditionEx<T> where T : Model;
internal record DateTimeEx<T>(string name, string op, string value) : IConditionEx<T> where T : Model;




