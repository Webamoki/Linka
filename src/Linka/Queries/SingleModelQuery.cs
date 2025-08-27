using System.Data;
using System.Linq.Expressions;
using System.Text.Json;
using Npgsql;
using Webamoki.Linka.ModelSystem;

namespace Webamoki.Linka.Queries;

internal class SingleModelQuery<T> where T : Model, new()
{
    private readonly IDbService _dbService;
    private readonly SelectQuery _query;
    private readonly List<NavigationInfo> _navigations = [];
    private readonly Dictionary<string,NavigationListInfo> _navigationLists = [];
    public SingleModelQuery(IDbService db,Expression<Func<T, bool>> expression)
    {
        if (!db.Schema.HasModel<T>()) throw new Exception($"Model {typeof(T).Name} not loaded for schema {db.Schema.Name}.");
        var condition = ModelQueryBuilder.Condition(expression, out var values, out var error);
        if (error != null)
            throw new Exception(error);
        
        _query = ModelQueryBuilder.GetQuery<T>();
        _query.SetCondition(condition, values);
        _dbService = db;
    }

    public SingleModelQuery(IDbService db, Expression<Func<T, bool>> expression, HashSet<string> navigations) : this(db, expression)
    {
        var info = ModelRegistry.Get<T>();
        var group = false;
        foreach (var navigation in navigations)
        {
            if (info.Navigations.TryGetValue(navigation, out var navInfo))
            {
                var targetInfo = navInfo.TargetModelInfo;
                _query.LeftJoin<T>(targetInfo.ModelType, navInfo.Field, navInfo.TargetField);
                foreach (var (fieldName, _) in targetInfo.Fields)
                {
                    _query.Select(targetInfo.ModelType, fieldName);
                }
                _navigations.Add(navInfo);
                continue;
            }
            if (info.NavigationLists.TryGetValue(navigation, out var navList))
            {
                group = true;
                var targetInfo = navList.TargetModelInfo;
                _query.LeftJoin<T>(targetInfo.ModelType, navList.Field, navList.TargetField);
                 _query.JsonSelect<T>(targetInfo.ModelType, navigation);
                _navigationLists.Add(navigation,navList);
                continue;
            }
            throw new Exception($"Navigation {navigation} not found in model {typeof(T).Name}.");
        }

        if (!group) return;
        foreach(var (field,_) in info.PrimaryFields)
        {
            _query.GroupBy<T>(field);
        }
    }
    public T First()
    {
        var model = FirstOrNull();
        if (model == null)
            throw new Exception($"Model {typeof(T).Name} not found.");
        return model;
    }

    public T? FirstOrNull()
    {
        _query.Limit = 1;
        var reader = _query.Execute(_dbService);
        return ReadModel(reader);

    }

    public T Single()
    {
        var model = SingleOrNull();
        if (model == null)
            throw new Exception($"Model {typeof(T).Name} not found.");
        return model;
    }

    public T? SingleOrNull()
    {
        var reader = _query.Execute(_dbService);
        // check if the reader has more than 1 row
        if (reader.FieldCount > 1)
            throw new Exception($"Query returned more than one row for model {typeof(T).Name}.");
        return ReadModel(reader);
    }
    
    private T? ReadModel(NpgsqlDataReader reader)
    {
        if (!reader.Read()) return null;
        var model = new T();
        model.Load<T>(reader);
        foreach (var navInfo in _navigations)
        {
            var targetInfo = navInfo.TargetModelInfo;
            var targetModel = targetInfo.Create();
            targetModel.Load(targetInfo.ModelType, reader);
            navInfo.Setter.Invoke(model, targetModel);
        }
        var tableName = Model.TableName<T>();
        foreach(var (column,navListInfo) in _navigationLists)
        {
            var json = reader.GetString($"{tableName}.{column}");
            if (string.IsNullOrEmpty(json)) continue;
            var list = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(json) ?? [];
            
            var targetInfo = navListInfo.TargetModelInfo;
            List<Model> models = [];
            foreach(var ipData in list)
            {
                var navModel = targetInfo.Create();
                navModel.Load(targetInfo.ModelType, ipData);
                models.Add(navModel);
            }
            if (models.Count == 0) continue;
            targetInfo.SetListNavigation(navListInfo.Setter, model, models);
            
        }
        reader.Close();
        _dbService.AddModelToCache(model);
        return model;
    }
}