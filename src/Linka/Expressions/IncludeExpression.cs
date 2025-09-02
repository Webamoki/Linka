using System.Linq.Expressions;
using Webamoki.Linka.ModelSystem;
using Webamoki.Linka.SchemaSystem;

namespace Webamoki.Linka.Expressions;

public class IncludeExpression<T, TSchema>
    where T : Model, new()
    where TSchema : Schema, new()
{
    private readonly DbService<TSchema> _dbService;
    private readonly HashSet<string> _included = [];

    internal IncludeExpression(DbService<TSchema> dbService, Expression<Func<T, object>> includeExpression)
    {
        _dbService = dbService;
        Include(includeExpression);
    }

    public void Include(Expression<Func<T, object>> includeExpression)
    {
        if (includeExpression.Body is MemberExpression fieldExpr)
        {
            var field = fieldExpr.Member.Name;
            if (!_included.Add(field)) throw new Exception($"Field {field} already included.");
        }
        else throw new Exception($"Invalid include expression: {includeExpression}");
    }

    public T Get(Expression<Func<T, bool>> expression) =>
        new GetExpression<T, TSchema>(_dbService, expression, _included).Get();

    public T? GetOrNull(Expression<Func<T, bool>> expression) =>
        new GetExpression<T, TSchema>(_dbService, expression, _included).GetOrNull();

    public GetManyExpression<T, TSchema> GetMany(Expression<Func<T, bool>> expression) =>
        new GetExpression<T, TSchema>(_dbService, expression, _included).GetMany();
}