using Webamoki.Linka.Expressions;
using Webamoki.Linka.Fields;
using Webamoki.Linka.Queries;

namespace Webamoki.Linka.ModelSystem;

internal class ModelUpdateRequest
{
    internal readonly Query PrimaryKey = new();
    internal readonly Dictionary<string, object?> ChangedFields = [];
    public ModelUpdateRequest(Model model)
    {
        var schema = model.DbService!.Schema;
        var info = ModelRegistry.Get(model.GetType());
        var tableName = info.TableName;
        foreach (var fieldKey in info.PrimaryFields.Keys)
        {
            var field = info.FieldGetters[fieldKey](model);
            if (!PrimaryKey.IsEmpty()) PrimaryKey.AddBody("AND");
            if (field.IsEmpty)
            {
                PrimaryKey.AddBody($"\"{tableName}\".\"{fieldKey}\" IS NULL");
            }
            else if (field is IEnumDbField enumField)
            {
                PrimaryKey.AddBody($"\"{tableName}\".\"{fieldKey}\" = '{field.StringValue()}'::\"{enumField.GetSchemaEnumName(schema)}\"");
            }
            else
            {
                PrimaryKey.AddBody($"\"{fieldKey}\" = ?");
                PrimaryKey.AddValue(field.ObjectValue());
            }
        }
    }
    public void AddSet(string field, object? value)
    {
        ChangedFields[field] = value;
    }
}
