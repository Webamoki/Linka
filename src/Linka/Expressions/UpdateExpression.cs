using System.Linq.Expressions;
using Webamoki.Linka.Fields;
using Webamoki.Linka.ModelSystem;
using Webamoki.Linka.Queries;
using Webamoki.Linka.SchemaSystem;

namespace Webamoki.Linka.Expressions;

public class UpdateExpression<T, TSchema> where T : Model, new() where TSchema : Schema, new()
{
    private readonly DbService<TSchema> _dbService;
    private readonly IEx<T> _expression;
    private readonly UpdateQuery _query;
    private readonly Dictionary<string, object?> _setFields = [];
    public UpdateExpression(DbService<TSchema> db, Expression<Func<T, bool>> expression)
    {
        var schema = Schema.Get<TSchema>();
        if (!schema.HasModel<T>()) throw new Exception($"Model {typeof(T).Name} not loaded for schema {schema.Name}.");
        var ex = ExParser.Parse(expression, out var error);
        if (error != null)
            throw new Exception(error);
        _expression = ex!;
        var condition = ex!.ToQuery(out var values);
        _query = new UpdateQuery(Model.TableName<T>());
        _query.SetCondition(condition, values);
        _dbService = db;
    }
    public UpdateExpression<T, TSchema> Set<TField>(Expression<Func<T, TField>> field, object? value) where TField : DbField
    {
        if (field.Body is MemberExpression fieldExpr)
        {
            var fieldName = fieldExpr.Member.Name;
            var fieldObj = ModelRegistry.Get<T>().Fields.TryGetValue(fieldName, out var f) ? f : throw new Exception($"Field {fieldName} not found in model {typeof(T).Name}");
            if (!fieldObj.IsValid(value, out var message)) throw new FormatException($"Invalid value for field {fieldName}: {message}");

            if (!_setFields.TryAdd(fieldName, value)) throw new Exception($"Field {fieldName} already set.");
            _query.AddSet<TSchema>(fieldObj, value);
        }
        else throw new Exception($"Invalid set expression: {field}");

        return this;
    }

    public void Save()
    {
        var code = _query.ExecuteTransaction(_dbService);
        if (code != DatabaseCode.Success) throw new InvalidOperationException($"Update failed with code {code}.");

        var cache = (ModelCache<T>)_dbService.GetModelCache<T>();
        cache.Update(_expression, _setFields);
    }
}