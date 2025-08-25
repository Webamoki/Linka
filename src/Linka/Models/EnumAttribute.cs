namespace Webamoki.Linka.Models;


internal interface IEnumAttribute
{
    void RegisterWithSchema<TDbSchema>()
        where TDbSchema : DbSchema, new();
    void Register<TDbSchema>() where TDbSchema : DbSchema, new();
}

[AttributeUsage(AttributeTargets.Constructor,AllowMultiple = true)]
public class EnumAttribute<T> : Attribute, IEnumAttribute
    where T : Enum, new()
{
    public EnumAttribute(string tableName) { Setup(tableName); }

    public EnumAttribute()
    {
        var name = typeof(T).Name;
        if (name.EndsWith("Model"))
            name = name[..^5];
        Setup(name);
    }

    private static void Setup(string tableName)
    {
        Model.SetTableName<T>(tableName);
        ModelRegistry.InitialRegister<T>();
    }


    public void RegisterWithSchema<TDbSchema>()
        where TDbSchema : DbSchema, new()
    {
        var schema = DbSchema.Get<TDbSchema>();
        schema.SchemaGeneric = new DbSchemaGeneric<TDbSchema>();
        schema.Models.Add(typeof(T));
        if (!DbSchema.ModelSchemas.TryAdd(typeof(T), schema))
            throw new Exception($"Model {typeof(T).Name} is already registered with a different DbSchema.");
    }
    
    public void Register<TDbSchema>()
        where TDbSchema : DbSchema, new()
    {
        var schema = DbSchema.Get<TDbSchema>();
        ModelRegistry.ApplyNavigations<T>(schema);
    }
}