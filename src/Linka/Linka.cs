using Testcontainers.PostgreSql;
using Webamoki.Linka.SchemaSystem;

namespace Webamoki.Linka;

public static class Linka
{
    public static bool Debug { internal get; set; }
    private static readonly HashSet<Type> CompiledSchemas = [];
    private static readonly Dictionary<string, string> DatabaseConnections = [];
    private static readonly Dictionary<string, string> SchemaDatabases = [];
    
    internal static string ConnectionString<T>() where T : Schema, new()
    {
        var schema = Schema.Get<T>();
        if (!SchemaDatabases.TryGetValue(schema.Name, out var database))
            throw new Exception($"No database configured for schema {schema.Name}.");
        if (!DatabaseConnections.TryGetValue(database, out var connectionString))
            throw new Exception($"No connection string configured for database {database}.");
        return connectionString;
    }
    public static bool TryCompile<T>() where T : Schema, new()
    {
        if (!CompiledSchemas.Add(typeof(T)))
            return false;
        var constructors = typeof(T).GetConstructors();
        List<ISchemaCompileAttribute> schemaAttributes = [];
        foreach (var ctor in constructors)
        {
            var attributes = ctor.GetCustomAttributes(typeof(ISchemaCompileAttribute), true);
            foreach (var attribute in attributes.Cast<ISchemaCompileAttribute>())
            {
                schemaAttributes.Add(attribute);
                attribute.Compile<T>();
            }
        }

        if (schemaAttributes.Count == 0)
            throw new Exception($"No ModelAttribute found for Schema {typeof(T).Name}. " +
                                "Please add a ModelAttribute to the constructor of the Schema class.");
        foreach (var attribute in schemaAttributes)
        {
            attribute.CompileConnections<T>();
        }

        return true;
    }
    
    public static void Configure<T>(string database) where T : Schema, new()
    {
        if (!TryCompile<T>())
            throw new Exception($"Schema {typeof(T).Name} is already registered.");
        Register<T>(database);
        Schema.Verify<T>();
    }
    
    public static void Register<T>(string database) where T : Schema, new()
    {
        var schema = Schema.Get<T>();
        SchemaDatabases[schema.Name] = database;
    }

    //
    // public static void Reset()
    // {
    //     ModelCache.Reset();
    // }

    public static void AddConnection(string server, string database, string user, string password, ushort port = 5432,bool includeDetails = false)
    {
        var connectionString = $"Server={server};Port={port};Database={database};User Id={user};Password={password};Include Error Detail={includeDetails}";
        if (!DatabaseConnections.TryAdd(database, connectionString))
            throw new Exception($"Connection string for database {database} already exists.");
    }
    
    public static void ForceConnection(string server, string database, string user, string password, ushort port = 5432 ,bool includeDetails = false)
    {
        var connectionString = $"Server={server};Port={port};Database={database};User Id={user};Password={password};Include Error Detail={includeDetails}";
        DatabaseConnections[database] = connectionString;
    }
}