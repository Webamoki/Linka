using Npgsql;

namespace Webamoki.Linka.Queries;

internal abstract class BaseQuery
{
    public abstract bool IsEmpty();
    public static implicit operator BaseQuery(string s) => new RenderedQuery(s);
}

internal class RenderedQuery(string query) : BaseQuery
{
    public readonly string Query = query;

    public override bool IsEmpty() => string.IsNullOrEmpty(Query);
}

internal class Query : BaseQuery
{
    private readonly List<object> _values = [];
    private List<BaseQuery> _body = [];


    public Query() { }

    public Query(string query) : this()
    {
        if (query.Length > 0)
            _body.Add(query);
    }

    public override bool IsEmpty()
    {
        if (_body.Count == 0)
            return true;
        foreach (var query in _body)
            if (!query.IsEmpty())
                return false;

        return true;
    }

    public void ResetBody() { _body = []; }

    public void AddValue(string value) => _values.Add(value);
    public virtual void AddValues(List<object> value) => _values.AddRange(value);
    public void PrependValue(string value) => _values.Insert(0, value);


    


    public void AddBody(params BaseQuery[] bodies)
    {
        foreach (var body in bodies)
            if (!body.IsEmpty())
                _body.Add(body);
    }

    public DatabaseCode ExecuteTransaction(IDbService service)
    {
        var query = Render(out var values);
        return service.ExecuteTransaction(query, values);
    }

    public NpgsqlDataReader Execute(IDbService service)
    {
        var query = Render(out var values);
        return service.Execute(query, values);
    }
    
    internal virtual string Render(out List<object> values)
    {
        var query = "";
        values = [];
        if (_body.Count == 0)
            return query;
        var localValueIndex = 0;
        foreach (var b in _body)
        {
            string partQuery;
            if (b is RenderedQuery renderedQuery)
            {
                partQuery = renderedQuery.Query;
                var valueCount = partQuery.Count(c => c == '?');
                if (valueCount > _values.Count)
                    throw new Exception("Not enough values to execute query.");
                if (valueCount > 0)
                    for (var i = 0; i < valueCount; i++)
                    {
                        values.Add(_values[localValueIndex]);
                        localValueIndex++;
                    }
            }
            else
            {
                partQuery = ((Query)b).Render(out var partValues);
                values.AddRange(partValues);
            }

            if (partQuery == "")
                continue;
            if (query != "")
                query += ' ';
            query += partQuery;
        }

        return query;
    }
}