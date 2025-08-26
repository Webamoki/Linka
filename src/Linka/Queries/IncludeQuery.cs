using System.Linq.Expressions;
using Webamoki.Linka.ModelSystem;

namespace Webamoki.Linka.Queries;

public class IncludeQuery<T>
    where T : Model, new()
{
    private readonly IDbService _dbService;
    private readonly HashSet<string> _included = [];

    public IncludeQuery(IDbService dbService, Expression<Func<T, object>> includeExpression)
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

    public T First(Expression<Func<T, bool>> expression) =>
        new SingleModelQuery<T>(_dbService, expression, _included).First();

    public T? FirstOrNull(Expression<Func<T, bool>> expression) =>
        new SingleModelQuery<T>(_dbService, expression, _included).FirstOrNull();

    public T Single(Expression<Func<T, bool>> expression) =>
        new SingleModelQuery<T>(_dbService, expression, _included).Single();

    public T? SingleOrNull(Expression<Func<T, bool>> expression) =>
        new SingleModelQuery<T>(_dbService, expression, _included).SingleOrNull();
}