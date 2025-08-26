using Webamoki.Linka.Fields;

namespace Webamoki.Linka.ModelSystem;

/// <summary>
/// Custom iterator for model field getters that provides efficient iteration
/// without directly accessing the underlying dictionary each time.
/// </summary>
internal class FieldIterator
{
    private readonly Dictionary<string, Func<Model, DbField>> _fieldGetters;
    private readonly Model _model;
    
    internal FieldIterator(Dictionary<string, Func<Model, DbField>> fieldGetters, Model model)
    {
        _fieldGetters = fieldGetters;
        _model = model;
    }
    
    /// <summary>
    /// Iterates through all fields in the model.
    /// </summary>
    public IEnumerable<(string FieldName, DbField Field)> All()
    {
        var info = ModelRegistry.Get(_model.GetType());
        foreach (var (fieldName, fieldGetter) in _fieldGetters)
        {
            var field = fieldGetter(_model);
            field.IsPrimary = info.Fields[fieldName].IsPrimary;
            field.IsUnique = info.Fields[fieldName].IsUnique;
            field.IsRequired = info.Fields[fieldName].IsRequired;
            field.Search = info.Fields[fieldName].Search;
            yield return (fieldName, field);
        }
    }
}
