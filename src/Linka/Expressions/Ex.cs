using Webamoki.Linka.ModelSystem;

namespace Webamoki.Linka.Expressions;

internal interface IEx<in T> where T : Model
{
    public string ToQuery(out List<object> values);
    
    public bool Verify(T model);
}
internal record Ex<T>(IEx<T> Left, bool IsAnd, IEx<T> Right) : IEx<T> where T : Model
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
    
    public bool Verify(T model) => Left.Verify(model) && Right.Verify(model);
}

internal abstract record ConditionEx<T>(string Name) : IEx<T> where T : Model
{
    protected string GetName()
    {
        var table = Model.TableName<T>();
        return $"\"{table}\".\"{Name}\"";
    }

    protected object GetValue(T model)
    {
        var info = ModelRegistry.Get<T>();
        return info.FieldGetters[Name](model).ObjectValue();
    }
    public abstract string ToQuery(out List<object> values);

    public abstract bool Verify(T model);
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

    public override bool Verify(T model)
    {
        var value = (string)GetValue(model);
        return IsEqual ? value == Value : value != Value;
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
    public override bool Verify(T model)
    {
        var value = GetValue(model);
        return IsEqual ? value != null : value == null;
    }
}







