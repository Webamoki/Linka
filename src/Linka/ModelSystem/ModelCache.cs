using Webamoki.Linka.Expressions;

namespace Webamoki.Linka.ModelSystem;

interface IModelCache
{
    void Add(Model model);
}

internal class ModelCache<T> : IModelCache where T : Model
{
    private readonly Dictionary<string, T> _primaryCache = [];
    private readonly Dictionary<string, T> _uniqueCache = [];

    public void Add(Model model)
    {
        var info = ModelRegistry.Get<T>();
        var primaryKey = GetPrimaryKey((T)model);
        _primaryCache[primaryKey] = (T)model;

        foreach (var (fieldKey, field) in info.UniqueFields)
        {
            if (!field.IsSet) continue;
            var uniqueKey = GetUniqueKey((T)model, fieldKey);
            _uniqueCache[uniqueKey] = (T)model;
        }
    }
    internal T? GetModel(IEx<T> ex)
    {
        // improve algorithm
        foreach (var (_, model) in _primaryCache)
        {
            if (ex.Verify(model))
                return model;
        }
        return null;
    }

    internal void Delete(IEx<T> ex)
    {
        foreach (var (_, model) in _primaryCache)
        {
            if (ex.Verify(model))
                Delete(model);
        }
    }

    private void Delete(T model)
    {
        var info = ModelRegistry.Get<T>();
        var primaryKey = GetPrimaryKey(model);
        _primaryCache.Remove(primaryKey);
        foreach (var (fieldKey, field) in info.UniqueFields)
        {
            if (!field.IsSet) continue;
            var uniqueKey = GetUniqueKey(model, fieldKey);
            _uniqueCache.Remove(uniqueKey);
        }
    }

    private string GetPrimaryKey(T model)
    {
        var info = ModelRegistry.Get<T>();
        var primaryKeys = new Dictionary<string, string>();
        foreach (var fieldKey in info.PrimaryFields.Keys)
        {
            var fieldValue = info.FieldGetters[fieldKey](model).StringValue();
            primaryKeys[fieldKey] = fieldValue;
        }
        // turn primaryKeys dictionary into string using both keys and values
        return string.Join(",", primaryKeys.Select(kvp => kvp.Key + "=" + kvp.Value));
    }

    private string GetUniqueKey(T model, string fieldKey)
    {
        var info = ModelRegistry.Get<T>();
        var fieldValue = info.FieldGetters[fieldKey](model).StringValue();
        return fieldKey + "=" + fieldValue;
    }

    internal void Update(IEx<T> ex, Dictionary<string, object?> setFields)
    {
        foreach (var (_, model) in _primaryCache)
        {
            if (ex.Verify(model))
                Update(model, setFields);
        }
    }

    private void Update(T model, Dictionary<string, object?> setFields)
    {
        var info = ModelRegistry.Get<T>();
        foreach (var (fieldKey, fieldValue) in setFields)
        {
            var field = info.FieldGetters[fieldKey](model);
            field.LoadValue(fieldValue);
            field.ResetChange();
        }
    }
}