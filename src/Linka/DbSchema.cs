using Testcontainers.PostgreSql;
using Webamoki.Linka.Models;

namespace Webamoki.Linka;

public class DbSchema
{
    internal readonly HashSet<Type> Models = [];
    internal static readonly Dictionary<Type, DbSchema> ModelSchemas = [];
    private static readonly Dictionary<Type, DbSchema> Instances = [];
    internal IDbSchemaGeneric? SchemaGeneric = null;
    internal string ConnectionString => Linka.GetConnectionString(DatabaseName);
    internal readonly string DatabaseName;
    internal readonly string Name;

    public bool HasModel<T>() where T : Model => HasModel(typeof(T));

    public bool HasModel(Type modelType) => Models.Contains(modelType);

    // ReSharper disable once MemberCanBeProtected.Global
    public DbSchema(string database, string name)
    {
        if (Instances.TryGetValue(GetType(), out _))
            throw new Exception("DbSchema constructor should not be used. Use DbSchema.Get<T>() instead.");
        Instances[GetType()] = this;
        DatabaseName = database;
        Name = name;
    }

    internal static void Verify<T>() where T : DbSchema, new()
    {
        var schema = Get<T>();
        foreach (var modelType in schema.Models)
        {
            ModelVerifier.Verify<T>(modelType);
        }
    }

    internal static DbSchema Get<T>() where T : DbSchema, new() =>
        Instances.TryGetValue(typeof(T), out var instance) ? instance : new T();
    
    internal static DbSchema GetWithModel<T>() where T : Model=>
        ModelSchemas.TryGetValue(typeof(T), out var schema) ? schema : throw new Exception($"Model {typeof(T).Name} is not registered with any DbSchema.");
    
}

internal interface IDbSchemaGeneric
{
    public PostgreSqlContainer Mock();
}

internal class DbSchemaGeneric<T> : IDbSchemaGeneric where T : DbSchema, new()
{
    public PostgreSqlContainer Mock() =>
        DbMocker.Mock<T>();
}