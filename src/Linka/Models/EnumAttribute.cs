namespace Webamoki.Linka.Models;




[AttributeUsage(AttributeTargets.Constructor,AllowMultiple = true)]
public class EnumAttribute<T> : Attribute, ISchemaRegisterAttribute
    where T : Enum, new()
{
    public void Register<TDbSchema>()
        where TDbSchema : DbSchema, new()
    {
        var schema = DbSchema.Get<TDbSchema>();
        if (schema.Enums.ContainsKey(typeof(T)))
            throw new Exception($"Enum {typeof(T).Name} is already registered for database {schema.DatabaseName}.");
        var name = typeof(T).Name;
        if (!name.EndsWith("Enum"))
            name += "Enum";
        schema.Enums.Add(typeof(T), (name, GetSqlType()));
    }
    private static string GetSqlType()
    {
        return $"ENUM ({string.Join(",", Enum.GetNames(typeof(T)).Select(name => $"'{name}'"))})";
    }
    public void RegisterConnections<TDbSchema>() where TDbSchema : DbSchema, new()
    {
    }
}