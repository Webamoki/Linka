using System.Linq.Expressions;
using Webamoki.Linka.ModelSystem;
using Webamoki.Linka.Queries;
using Webamoki.Linka.SchemaSystem;

namespace Webamoki.Linka.Expressions;

internal class DeleteExpression<T, TSchema> where T : Model, new() where TSchema : Schema, new()
{
    private readonly DbService<TSchema> _dbService;
    private readonly IEx<T> _expression;
    private readonly DeleteQuery _query;
    public DeleteExpression(DbService<TSchema> db, Expression<Func<T, bool>> expression)
    {
        var schema = Schema.Get<TSchema>();
        if (!schema.HasModel<T>()) throw new Exception($"Model {typeof(T).Name} not loaded for schema {schema.Name}.");
        var ex = ExParser.Parse(expression, out var error);
        if (error != null)
            throw new Exception(error);
        _expression = ex!;
        var condition = ex!.ToQuery(out var values);
        _query = new DeleteQuery(Model.TableName<T>());
        _query.SetCondition(condition, values);
        _dbService = db;
    }

    public void Delete()
    {
        var code = _query.ExecuteTransaction(_dbService);
        if (code != DatabaseCode.Success) throw new InvalidOperationException($"Delete failed with code {code}.");

        var cache = (ModelCache<T>)_dbService.GetModelCache<T>();
        cache.Delete(_expression);
    }
}