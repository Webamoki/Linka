using System.Data;
using System.Linq.Expressions;
using System.Text.Json;
using Npgsql;
using Webamoki.Linka.ModelSystem;
using Webamoki.Linka.Queries;
using Webamoki.Linka.SchemaSystem;

namespace Webamoki.Linka.Expressions;

public class GetExpression<T, TSchema> where T : Model, new() where TSchema : Schema, new()
{
    internal readonly DbService<TSchema> DbService;
    internal readonly SelectQuery Query;
    internal readonly IEx<T> Expression;
    private readonly List<NavigationInfo> _navigations = [];
    private readonly Dictionary<string, NavigationListInfo> _navigationLists = [];
    internal GetExpression(DbService<TSchema> db, Expression<Func<T, bool>> expression)
    {
        if (!db.Schema.HasModel<T>()) throw new Exception($"Model {typeof(T).Name} not loaded for schema {db.Schema.Name}.");
        var ex = ExParser.Parse(expression, out var error);
        if (error != null)
            throw new Exception(error);
        Expression = ex!;

        var condition = ex!.ToQuery(out var values);

        Query = GetQuery();
        Query.SetCondition(condition, values);
        DbService = db;
    }

    internal static SelectQuery GetQuery()
    {
        var query = new SelectQuery();
        query.AddTable<T>();
        var modelInfo = ModelRegistry.Get<T>();
        foreach (var field in modelInfo.Fields.Values)
        {
            query.Select<T>(field);
        }

        return query;
    }

    internal GetExpression(DbService<TSchema> db, Expression<Func<T, bool>> expression, HashSet<string> navigations) : this(db, expression)
    {
        var info = ModelRegistry.Get<T>();
        var group = false;
        foreach (var navigation in navigations)
        {
            if (info.Navigations.TryGetValue(navigation, out var navInfo))
            {
                var targetInfo = navInfo.TargetModelInfo;
                Query.LeftJoin<T>(targetInfo.ModelType, navInfo.Field, navInfo.TargetField);
                foreach (var field in targetInfo.Fields.Values)
                {
                    Query.Select(targetInfo.ModelType, field);
                }
                _navigations.Add(navInfo);
                continue;
            }
            if (info.NavigationLists.TryGetValue(navigation, out var navList))
            {
                group = true;
                var targetInfo = navList.TargetModelInfo;
                Query.LeftJoin<T>(targetInfo.ModelType, navList.Field, navList.TargetField);
                Query.JsonSelect<T>(targetInfo.ModelType, navigation);
                _navigationLists.Add(navigation, navList);
                continue;
            }
            throw new Exception($"Navigation {navigation} not found in model {typeof(T).Name}.");
        }

        if (!group) return;
        foreach (var (field, _) in info.PrimaryFields)
        {
            Query.GroupBy<T>(field);
        }
    }
    internal T Get()
    {
        var model = GetOrNull();
        if (model == null)
            throw new Exception($"Model {typeof(T).Name} not found.");
        return model;
    }

    internal T? GetOrNull()
    {
        Query.Limit = 1;
        if (_navigationLists.Count == 0 && _navigations.Count == 0)
        {
            var cachedModel = GetCachedModel();
            if (cachedModel != null) return cachedModel;
        }
        var reader = Query.Execute(DbService);
        if (!reader.Read())
        {
            reader.Close();
            return null;
        }
        var model = ReadModel(reader);
        reader.Close();
        return model;
    }

    private T? GetCachedModel()
    {
        var cache = (ModelCache<T>)DbService.GetModelCache<T>();
        return cache.GetModel(Expression);
    }

    public GetManyExpression<T, TSchema> GetMany() => new(this);

    internal T ReadModel(NpgsqlDataReader reader)
    {
        var model = new T();
        model.Load<T>(reader);
        foreach (var navInfo in _navigations)
        {
            var targetInfo = navInfo.TargetModelInfo;
            var targetModel = targetInfo.Create();
            targetModel.Load(targetInfo.ModelType, reader);
            DbService.AddModelToCache(targetModel);
            navInfo.Setter.Invoke(model, targetModel);
        }
        var tableName = Model.TableName<T>();
        foreach (var (column, navListInfo) in _navigationLists)
        {
            var json = reader.GetString($"{tableName}.{column}");
            if (string.IsNullOrEmpty(json)) continue;
            var list = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(json) ?? [];

            var targetInfo = navListInfo.TargetModelInfo;
            List<Model> models = [];
            foreach (var navData in list)
            {
                var navModel = targetInfo.Create();
                navModel.Load(targetInfo.ModelType, navData);
                models.Add(navModel);
                DbService.AddModelToCache(navModel);
            }
            if (models.Count == 0) continue;
            targetInfo.SetListNavigation(navListInfo.Setter, model, models);

        }
        DbService.AddModelToCache(model);
        return model;
    }
}