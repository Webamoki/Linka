namespace Webamoki.Linka.Queries;

internal class DeleteQuery(string table, string? alias = null) : ConditionQuery
{
    private readonly string _table = alias != null ? $"\"{table}\" AS \"{alias}\"" : $"\"{table}\"";

    internal override string Render(out List<object> values)
    {
        ResetBody();
        AddBody($"DELETE FROM {_table}");
        if (!Condition.IsEmpty()) AddBody("WHERE", Condition);

        return base.Render(out values);
    }
}