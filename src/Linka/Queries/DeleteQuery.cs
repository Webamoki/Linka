
namespace Webamoki.Linka.Queries;

internal class DeleteQuery : ConditionQuery
{
    private string _table;
    
    public DeleteQuery(string table, string? alias = null)
    {
        _table = alias != null ? $"\"{table}\" AS \"{alias}\"" : $"\"{table}\"";
    }

    internal override string Render(out List<object> values)
    {
        ResetBody();
        AddBody($"DELETE FROM {_table}");
        if (!Condition.IsEmpty())
        {
            AddBody("WHERE", Condition);
        }

        return base.Render(out values);
    }
}