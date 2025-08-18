using System.Data;
using System.Text.Json;
using Npgsql;

namespace Webamoki.Linka.Models;

public abstract class Model
{
    private static readonly Dictionary<Type, string> TableNames = [];

    public static string TableName<T>() where T : Model => TableName(typeof(T));
    private static string TableName(Type type)
    {
        if (TableNames.TryGetValue(type, out var tableName))
            return tableName;
        throw new Exception($"Table name for {type.Name} not set.");
    }
    public string TableName() => TableName(GetType());


    public static void SetTableName<T>(string tableName)
    {
        var type = typeof(T);
        if (!TableNames.TryAdd(type, tableName))
            throw new Exception($"Table name for {type.Name} already set.");
    }

    public void Load<T>(NpgsqlDataReader reader) where T : Model { Load(typeof(T), reader); }

    public void Load(Type type, IDataReader reader)
    {
        var info = ModelRegistry.Get(type);
        foreach (var (fieldName, fieldGetter) in info.FieldGetters)
        {
            var value = reader[$"{info.TableName}.{fieldName}"];
            if (value is DBNull) value = null;
            var field = fieldGetter(this);
            field.LoadValue(value);
        }
    }

    public void Load(Type type, Dictionary<string, JsonElement> reader)
    {
        var info = ModelRegistry.Get(type);
        foreach (var (fieldName, fieldGetter) in info.FieldGetters)
        {
            var jsonElement = reader[$"{info.TableName}.{fieldName}"];
            object? value =  jsonElement.ValueKind switch
            {
                JsonValueKind.String => jsonElement.GetString(),
                JsonValueKind.Number => jsonElement.TryGetInt64(out var l) ? l : jsonElement.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => throw new NotSupportedException(
                    $"Unsupported JSON value kind: {jsonElement.ValueKind} for field {fieldName} in model {type.Name}.")
            };
            var field = fieldGetter(this);
            field.LoadValue(value);
        }
    }
}