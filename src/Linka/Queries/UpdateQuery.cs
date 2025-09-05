using Webamoki.Linka.Fields;
using Webamoki.Linka.SchemaSystem;

namespace Webamoki.Linka.Queries;

internal class UpdateQuery(string table, string? alias = null) : ConditionQuery
{
    private readonly Query _set = new();
    private readonly string _table = alias != null ? $"\"{table}\" AS \"{alias}\"" : $"\"{table}\"";
    public void AddSet<T>(DbField field, object? value) where T : Schema, new()
    {
        var column = field.Name;
        if (value is null)
            AddSetBody($"\"{column}\" = NULL");
        else
        {
            var set = field.GetUpdateSetQuery<T>(value, out var queryValue);
            AddSetBody($"\"{column}\" = {set}");
            if (queryValue != null) _set.AddValue(queryValue);
        }
    }
    public void AddSetBody(string column)
    {
        if (!_set.IsEmpty()) _set.AddBody(",");
        _set.AddBody(column);
    }

    public override bool IsEmpty() => base.IsEmpty() && _set.IsEmpty();
    internal override string Render(out List<object> values)
    {
        ResetBody();
        AddBody($"UPDATE {_table}");

        if (_set.IsEmpty())
            throw new Exception("UPDATE query must have at least one SET clause. Use AddSetColumn() to specify columns to update.");
        AddBody("SET", _set);

        if (!Condition.IsEmpty()) AddBody("WHERE", Condition);

        return base.Render(out values);
    }
}