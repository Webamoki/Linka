using Webamoki.Linka.Expressions;

namespace Webamoki.Linka.ModelSystem;

interface IModelCache
{
    void Add(Model model);
}

public class ModelCache<T> : IModelCache where T : Model
{
    private readonly Dictionary<Dictionary<string, string>, T> _primaryCache = [];
    private readonly Dictionary<(string,string), T> _uniqueCache = [];
    
    public void Add(Model model)
    {
        var info = ModelRegistry.Get<T>();
        var primaryKeys = new Dictionary<string, string>();
        foreach (var fieldKey in info.PrimaryFields.Keys)
        {
            
            var fieldValue = info.FieldGetters[fieldKey](model).StringValue();
            primaryKeys[fieldKey] = fieldValue;
        }
        _primaryCache[primaryKeys] = (T)model;
        foreach (var (fieldKey,field) in info.UniqueFields)
        {
            if (!field.IsSet) continue;
            var fieldValue = info.FieldGetters[fieldKey](model).StringValue();
            
            _uniqueCache[(fieldKey,fieldValue)] = (T)model;
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
        var primaryKeys = new Dictionary<string, string>();
        foreach (var fieldKey in info.PrimaryFields.Keys)
        {
            var fieldValue = info.FieldGetters[fieldKey](model).StringValue();
            primaryKeys[fieldKey] = fieldValue;
        }
        _primaryCache.Remove(primaryKeys);
        foreach (var (fieldKey,field) in info.UniqueFields)
        {
            if (!field.IsSet) continue;
            var fieldValue = info.FieldGetters[fieldKey](model).StringValue();
            _uniqueCache.Remove((fieldKey,fieldValue));
        }
    }
}