using Testcontainers.PostgreSql;
using Webamoki.Linka.Checks;
using Webamoki.Linka.Models;

namespace Webamoki.Linka;
internal interface ISchemaCompileAttribute
{
    void Compile<TDbSchema>()
        where TDbSchema : DbSchema, new();
    void CompileConnections<TDbSchema>() where TDbSchema : DbSchema, new();
}
public class DbSchema
{
    internal readonly HashSet<Type> Models = [];
    internal readonly Dictionary<Type, (string Name,string SqlType)> Enums = [];
    internal static readonly Dictionary<Type, DbSchema> ModelSchemas = [];
    private static readonly Dictionary<Type, DbSchema> Instances = [];
    internal IDbSchemaGeneric? SchemaGeneric = null;
    internal readonly string Name;

    public bool HasModel<T>() where T : Model => HasModel(typeof(T));

    public bool HasModel(Type modelType) => Models.Contains(modelType);
    
    public bool HasEnum<T>() where T : Enum => HasEnum(typeof(T));
    public bool HasEnum(Type enumType) => Enums.ContainsKey(enumType);
    public string GetEnumName<T>() where T : Enum => Enums[typeof(T)].Name;

    // ReSharper disable once MemberCanBeProtected.Global
    public DbSchema(string name)
    {
        if (Instances.TryGetValue(GetType(), out _))
            throw new Exception("DbSchema constructor should not be used. Use DbSchema.Get<T>() instead.");
        Instances[GetType()] = this;
        Name = name;
    }

    internal static void Verify<T>() where T : DbSchema, new()
    {
        var schema = Get<T>();

        // Verify the schema itself (enums, etc.)
        DbSchemaCheck.Check<T>();

        // Verify each model in the schema
        foreach (var modelType in schema.Models)
        {
            ModelCheck.Check<T>(modelType);
        }
    }

    internal static DbSchema Get<T>() where T : DbSchema, new() =>
        Instances.TryGetValue(typeof(T), out var instance) ? instance : new T();
    
    internal static DbSchema GetWithModel<T>() where T : Model=>
        ModelSchemas.TryGetValue(typeof(T), out var schema) ? schema : throw new Exception($"Model {typeof(T).Name} is not registered with any DbSchema.");
    
}

/// <summary>
/// Schema to Generic Translator without using Reflection.
/// Allows a Schema to store itself as a generic.
/// </summary>
internal interface IDbSchemaGeneric
{
    public PostgreSqlContainer Mock();
}

internal class DbSchemaGeneric<T> : IDbSchemaGeneric where T : DbSchema, new()
{
    public PostgreSqlContainer Mock() {
        var container = DbMocker.Mock<T>();
        DbSchema.Verify<T>();
        return container;
    }
}