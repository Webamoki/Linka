using Webamoki.Linka.ModelSystem;

namespace Webamoki.Linka.SchemaSystem;

[AttributeUsage(AttributeTargets.Constructor, AllowMultiple = true)]
public class ModelAttribute<T> : Attribute, ISchemaCompileAttribute
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
        ModelRegistry.InitialCompile<T>();
    }


    public void Compile<TSchema>()
        where TSchema : Schema, new()
    {
        var schema = Schema.Get<TSchema>();
        schema.SchemaGeneric = new SchemaGeneric<TSchema>();
        schema.Models.Add(typeof(T));
        if (!Schema.ModelSchemas.TryAdd(typeof(T), schema))
            throw new Exception($"Model {typeof(T).Name} is already registered with a different Schema.");
    }

    public void CompileConnections<TSchema>()
        where TSchema : Schema, new()
    {
        var schema = Schema.Get<TSchema>();
        ModelRegistry.ApplyNavigations<T>(schema);
    }
}