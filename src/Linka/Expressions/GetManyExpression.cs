using Webamoki.Linka.ModelSystem;
using Webamoki.Linka.Queries;

namespace Webamoki.Linka.Expressions;

public class GetManyExpression<T> where T : Model, new()
{
    private readonly GetExpression<T> _getExpression;
    internal GetManyExpression(GetExpression<T> getExpression)
    {
        _getExpression = getExpression;
    }
    public int Count()
    {
        _getExpression._query.IsCount = true;
        var reader = _getExpression._query.Execute( _getExpression._dbService);
        if (!reader.Read()) return 0;
        return reader.GetInt32(0);
    }
    
    public List<T> Load()
    {
        var reader =  _getExpression._query.Execute( _getExpression._dbService);
        List<T> models = [];
        while (reader.Read())
        {
            models.Add( _getExpression.ReadModel(reader));
        }
        reader.Close();

        return models;
    }
}