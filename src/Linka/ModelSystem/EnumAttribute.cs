using Webamoki.Linka.SchemaSystem;

namespace Webamoki.Linka.ModelSystem;




[AttributeUsage(AttributeTargets.Constructor,AllowMultiple = true)]
public class EnumAttribute<T> : Attribute, ISchemaCompileAttribute
    where T : Enum, new()
{
    public void Compile<TDbSchema>()
        where TDbSchema : Schema, new()
    {
        var schema = Schema.Get<TDbSchema>();
        if (schema.Enums.ContainsKey(typeof(T)))
            throw new Exception($"Enum {typeof(T).Name} is already registered for schema {schema.Name}.");
        var name = typeof(T).Name;
        if (!name.EndsWith("Enum"))
            name += "Enum";
        schema.Enums.Add(typeof(T), (name, GetSqlType()));
    }
    private static string GetSqlType()
    {
        return $"ENUM ({string.Join(",", Enum.GetNames(typeof(T)).Select(name => $"'{name}'"))})";
    }
    public void CompileConnections<TDbSchema>() where TDbSchema : Schema, new()
    {
    }
}