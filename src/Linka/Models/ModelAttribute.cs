namespace Webamoki.Linka.Models;

[AttributeUsage(AttributeTargets.Constructor,AllowMultiple = true)]
public class ModelAttribute<T> : Attribute, ISchemaRegisterAttribute
    where T : Model, new()
{
    public ModelAttribute(string tableName) { Setup(tableName); }

    public ModelAttribute()
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


    public void Register<TDbSchema>()
        where TDbSchema : DbSchema, new()
    {
        var schema = DbSchema.Get<TDbSchema>();
        schema.SchemaGeneric = new DbSchemaGeneric<TDbSchema>();
        schema.Models.Add(typeof(T));
        if (!DbSchema.ModelSchemas.TryAdd(typeof(T), schema))
            throw new Exception($"Model {typeof(T).Name} is already registered with a different DbSchema.");
    }
    
    public void RegisterConnections<TDbSchema>()
        where TDbSchema : DbSchema, new()
    {
        var schema = DbSchema.Get<TDbSchema>();
        ModelRegistry.ApplyNavigations<T>(schema);
    }
}