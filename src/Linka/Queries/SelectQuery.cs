using Webamoki.Linka.Fields;
using Webamoki.Linka.ModelSystem;

namespace Webamoki.Linka.Queries;

internal class SelectQuery : ConditionQuery
{
    private readonly Query _select = new();
    public int Offset { private get; set; }
    public int Limit { private get; set; }
    private readonly Query _orderBy = new();
    public bool IsCount = false;
    private readonly Query _join = new();
    private readonly Query _groupBy = new();
    private Query? _tables;
    private Query Tables => _tables ??= new Query();

    private void AddSelect(IQuery select)
    {
        if (!_select.IsEmpty())
        {
            _select.AddBody(",");
        }

        _select.AddBody(select);
    }

    // public void AddOrderBy(IQuery orderBy, bool descending = false)
    // {
    //     _orderBy.AddBody("ORDER BY", orderBy);
    //     if (descending)
    //     {
    //         _orderBy.AddBody("DESC");
    //     }
    // }

    public void GroupBy<T>(string column) where T : Model
    {
        var table = Model.TableName<T>();
        if (!_groupBy.IsEmpty())
        {
            _groupBy.AddBody(",");
        }

        _groupBy.AddBody($"\"{table}\".\"{column}\"");
    }

    public void Select<T>(DbField field) where T : Model
    {
        var table = Model.TableName<T>();
        FieldSelect(table,field);
    }
    public void Select(Type modelType, DbField field)
    {
        var table = ModelRegistry.Get(modelType).TableName;
        FieldSelect(table,field);
    }

    private void FieldSelect(string table, DbField field)
    {
        var column = field.Name;
        var cast = field is IEnumDbField ? "::text" : "";
        
        AddSelect($"\"{table}\".\"{column}\"{cast} as \"{table}.{column}\"");
    }

    public void JsonSelect<T>(Type modelType, string alias) where T : Model
    {
        var columns = "";
        var info = ModelRegistry.Get(modelType);
        var tableName = info.TableName;
        var lastField = info.Fields.LastOrDefault().Key;
        foreach (var (field, _) in info.Fields)
        {
            columns += $"'{tableName}.{field}',\"{tableName}\".\"{field}\",";
        }

        AddSelect($"""
                   COALESCE(
                       jsonb_agg(
                           jsonb_build_object(
                               {columns[..^1]}
                           )
                       ) FILTER (WHERE "{tableName}"."{lastField}" IS NOT NULL),
                       '[]'::jsonb
                   ) AS "{ModelRegistry.Get<T>().TableName}.{alias}"
                   """
        );
    }

    public void Select<T1, T2>(string column, string alias) where T1 : Model where T2 : Model
    {
        var table1 = Model.TableName<T1>();
        var table2 = Model.TableName<T2>();
        AddSelect($"\"{table1}\".\"{column}\" as \"{table2}.{alias}\"");
    }

    private void Join(string table1, string table2, string field1, string field2, string alias = "",
        string join = "JOIN")
    {
        string condition;
        if (alias != "")
        {
            condition = $"\"{table1}\".\"{field1}\"=\"{alias}\".\"{field2}\"";
            alias = $" AS \"{alias}\"";
        }
        else
        {
            condition = $"\"{table1}\".\"{field1}\"=\"{table2}\".\"{field2}\"";
        }

        _join.AddBody(join, $"\"{table2}\"{alias} ON", condition);
    }

    public void AddTable<T>(string? alias = null) where T : Model { AddTable(Model.TableName<T>(), alias); }

    public void AddTable(string table, string? alias = null)
    {
        var tables = Tables;
        if (!tables.IsEmpty()) tables.AddBody(",");
        tables.AddBody($"\"{table}\"");
        if (alias != null) tables.AddBody($"\"{alias}\"");
    }

    private void Join<T>(Type modelType, string field1, string field2, string alias = "", string join = "JOIN")
        where T : Model
    {
        var table1 = Model.TableName<T>();
        var table2 = ModelRegistry.Get(modelType).TableName;
        Join(table1, table2, field1, field2, alias, join);
    }

    public void Join<T1, T2>(string field1, string field2, string alias = "", string join = "JOIN")
        where T1 : Model where T2 : Model
    {
        var table1 = Model.TableName<T1>();
        var table2 = Model.TableName<T2>();
        Join(table1, table2, field1, field2, alias, join);
    }

    public void LeftJoin<T>(Type modelType, string field1, string field2, string alias = "") where T : Model
    {
        Join<T>(modelType, field1, field2, alias, "LEFT JOIN");
    }

    internal override string Render(out List<object> values)
    {
        if (Tables.IsEmpty()) throw new Exception("No tables specified");
        ResetBody();
        AddBody("SELECT");

        if (IsCount)
        {
            AddBody("COUNT(*)");
        }
        else if (!_select.IsEmpty())
        {
            AddBody(_select);
        }
        else
        {
            AddBody("*");
        }

        AddBody("FROM", Tables);
        if (!_join.IsEmpty())
        {
            AddBody(_join);
        }

        if (!Condition.IsEmpty())
        {
            AddBody("WHERE", Condition);
        }

        if (!_groupBy.IsEmpty())
        {
            AddBody("GROUP BY", _groupBy);
        }

        if (!IsCount)
        {
            if (!_orderBy.IsEmpty())
            {
                AddBody(_orderBy);
            }

            if (Limit > 0)
            {
                AddBody($"LIMIT {Limit}");
            }

            if (Offset > 0)
            {
                AddBody($"OFFSET {Offset}");
            }
        }

        return base.Render(out values);
    }
}