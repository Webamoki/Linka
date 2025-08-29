using System.Linq.Expressions;
using Webamoki.Linka.ModelSystem;
using Webamoki.Linka.Queries;

namespace Webamoki.Linka.Expressions;

internal class DeleteExpression<T> where T : Model, new()
{
    private readonly IDbService _dbService;
    private readonly DeleteQuery _query;
    public DeleteExpression(IDbService db,Expression<Func<T, bool>> expression)
    {
        if (!db.Schema.HasModel<T>()) throw new Exception($"Model {typeof(T).Name} not loaded for schema {db.Schema.Name}.");
        var condition = ExpressionBuilder.Condition(expression, out var values, out var error);
        if (error != null)
            throw new Exception(error);

        _query = new DeleteQuery(Model.TableName<T>());
        _query.SetCondition(condition, values);
        _dbService = db;
    }


    public void Delete()
    {
        var code = _query.ExecuteTransaction(_dbService);
        if (code != DatabaseCode.Success)
        {
            throw new InvalidOperationException($"Delete failed with code {code}.");
        }
        
    }
}