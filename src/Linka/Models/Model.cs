using System.Data;
using System.Text.Json;
using Npgsql;
using Webamoki.Linka.Fields;
using Webamoki.Linka.Queries;

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

    internal FieldIterator GetFieldIterator()
    {
        var info = ModelRegistry.Get(GetType());
        return info.FieldIterator(this);
    }

    public static void SetTableName<T>(string tableName)
    {
        var type = typeof(T);
        if (!TableNames.TryAdd(type, tableName))
            throw new Exception($"Table name for {type.Name} already set.");
    }

    internal void Load<T>(NpgsqlDataReader reader) where T : Model { Load(typeof(T), reader); }
    
    internal void Load(Type type, IDataReader reader)
    {
        var info = ModelRegistry.Get(type);
        var fieldIterator = GetFieldIterator();
        foreach (var (fieldName, field) in fieldIterator.All())
        {
            var value = reader[$"{info.TableName}.{fieldName}"];
            if (value is DBNull) value = null;
            field.LoadValue(value);
        }
    }

    internal void Load(Type type, Dictionary<string, JsonElement> reader)
    {
        var info = ModelRegistry.Get(type);
        var fieldIterator = GetFieldIterator();
        foreach (var (fieldName, field) in fieldIterator.All())
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
            field.LoadValue(value);
        }
    }
}