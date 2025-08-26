using Testcontainers.PostgreSql;
using Webamoki.Linka.ModelSystem;
using Webamoki.Linka.Testing;

namespace Webamoki.Linka.SchemaSystem;
internal interface ISchemaCompileAttribute
{
    void Compile<TSchema>()
        where TSchema : Schema, new();
    void CompileConnections<TSchema>() where TSchema : Schema, new();
}
public class Schema
{
    internal readonly HashSet<Type> Models = [];
    internal readonly Dictionary<Type, (string Name,string SqlType)> Enums = [];
    internal static readonly Dictionary<Type, Schema> ModelSchemas = [];
    private static readonly Dictionary<Type, Schema> Instances = [];
    internal ISchemaGeneric? SchemaGeneric = null;
    internal readonly string Name;

    public bool HasModel<T>() where T : Model => HasModel(typeof(T));

    public bool HasModel(Type modelType) => Models.Contains(modelType);
    
    public bool HasEnum<T>() where T : Enum => HasEnum(typeof(T));
    public bool HasEnum(Type enumType) => Enums.ContainsKey(enumType);
    public string GetEnumName<T>() where T : Enum => Enums[typeof(T)].Name;

    // ReSharper disable once MemberCanBeProtected.Global
    public Schema(string name)
    {
        if (Instances.TryGetValue(GetType(), out _))
            throw new Exception("Schema constructor should not be used. Use Schema.Get<T>() instead.");
        Instances[GetType()] = this;
        Name = name;
    }

    internal static void Verify<T>() where T : Schema, new()
    {
        var schema = Get<T>();

        // Verify the schema itself (enums, etc.)
        SchemaCheck.Check<T>();

        // Verify each model in the schema
        foreach (var modelType in schema.Models)
        {
            ModelCheck.Check<T>(modelType);
        }
    }

    internal static Schema Get<T>() where T : Schema, new() =>
        Instances.TryGetValue(typeof(T), out var instance) ? instance : new T();
    
    internal static Schema GetWithModel<T>() where T : Model=>
        ModelSchemas.TryGetValue(typeof(T), out var schema) ? schema : throw new Exception($"Model {typeof(T).Name} is not registered with any Schema.");
    
}

/// <summary>
/// Schema to Generic Translator without using Reflection.
/// Allows a Schema to store itself as a generic.
/// </summary>
internal interface ISchemaGeneric
{
    public PostgreSqlContainer Mock();
}

internal class SchemaGeneric<T> : ISchemaGeneric where T : Schema, new()
{
    public PostgreSqlContainer Mock() {
        var container = DbMocker.Mock<T>();
        Schema.Verify<T>();
        return container;
    }
}