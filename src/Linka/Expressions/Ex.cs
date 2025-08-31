using Webamoki.Linka.ModelSystem;

namespace Webamoki.Linka.Expressions;

internal interface IEx<T> where T : Model
{
    public string ToQuery(out List<object> values);
}
internal record Ex<T>(IEx<T> Left, bool IsAnd, IEx<T> Right) :  IEx<T> where T : Model
{
    public string ToQuery(out List<object> values)
    {
        var leftQuery = Left.ToQuery(out var leftValues);
        var rightQuery = Right.ToQuery(out var rightValues);
        values = leftValues;
        values.AddRange(rightValues);
        var op = IsAnd ? "AND" : "OR";
        return $"({leftQuery} {op} {rightQuery})";
    }
}

internal abstract record ConditionEx<T>(string Name) : IEx<T> where T : Model
{
    protected string GetName()
    {
        var table = Model.TableName<T>();
        return $"\"{table}\".\"{Name}\"";
    }

    public abstract string ToQuery(out List<object> values);
}

internal record StringEx<T>(string Name, bool IsEqual, string Value, bool IsInjectable) : ConditionEx<T>(Name) where T : Model
{
    public override string ToQuery(out List<object> values)
    {
        var op = IsEqual ? "=" : "!=";
        if (IsInjectable)
        {
            values = [Value];
            return $"{GetName()} {op} ?";
        }
        values = [];
        return $"{GetName()} {op} '{Value}'";
    }
}
internal record NullEx<T>(string Name, bool IsEqual) : ConditionEx<T>(Name) where T : Model
{
    public override string ToQuery(out List<object> values)
    {
        var op = IsEqual ? "IS NOT" : "IS";
        values = [];
        return $"{GetName()} {op} NULL";
    }
}







