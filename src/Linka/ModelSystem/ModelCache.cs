using Webamoki.Linka.Expressions;

namespace Webamoki.Linka.ModelSystem;

interface IModelCache
{
    void Add(Model model);
    Model? Find(IEx)
}

public class ModelCache<T> : IModelCache where T : Model
{
    private readonly Dictionary<Dictionary<string, string>, Model> _primaryCache = [];
    private readonly Dictionary<(string,string), Model> _uniqueCache = [];
    
    public void Add(Model model)
    {
        var info = ModelRegistry.Get<T>();
        var primaryKeys = new Dictionary<string, string>();
        foreach (var fieldKey in info.PrimaryFields.Keys)
        {
            
            var fieldValue = info.FieldGetters[fieldKey](model).StringValue();
            primaryKeys[fieldKey] = fieldValue;
        }
        _primaryCache[primaryKeys] = model;
        foreach (var (fieldKey,field) in info.UniqueFields)
        {
            if (!field.IsSet) continue;
            var fieldValue = info.FieldGetters[fieldKey](model).StringValue();
            
            _uniqueCache[(fieldKey,fieldValue)] = model;
        }
    }
    
    public Model? GetUnique(string fieldKey, string fieldValue)
    {
        return _uniqueCache.TryGetValue((fieldKey, fieldValue), out var model) ? model : null;
    }
}