using Webamoki.Linka.ModelSystem;
using Webamoki.Linka.SchemaSystem;

namespace Webamoki.Linka.Expressions;

public class GetManyExpression<T, TSchema> where T : Model, new() where TSchema : Schema, new()
{
    private readonly GetExpression<T, TSchema> _getExpression;
    internal GetManyExpression(GetExpression<T, TSchema> getExpression)
    {
        _getExpression = getExpression;
    }
    public int Count()
    {
        _getExpression.Query.IsCount = true;
        var reader = _getExpression.Query.Execute(_getExpression.DbService);
        if (!reader.Read()) return 0;
        return reader.GetInt32(0);
    }

    public List<T> Load()
    {
        _getExpression.Query.IsCount = false;
        var reader = _getExpression.Query.Execute(_getExpression.DbService);
        List<T> models = [];
        while (reader.Read())
        {
            models.Add(_getExpression.ReadModel(reader));
        }
        reader.Close();

        return models;
    }
}