using Testcontainers.PostgreSql;
using Webamoki.Linka.Models;

namespace Webamoki.Linka;

public static class Linka
{
    public static bool Debug { internal get; set; }
    private static readonly HashSet<Type> RegisteredSchemas = [];
    private static readonly Dictionary<string, string> ConnectionStrings = [];
    public static bool TryRegister<T>() where T : DbSchema, new()
    {
        if (!RegisteredSchemas.Add(typeof(T)))
            return false;
        var constructors = typeof(T).GetConstructors();
        List<ISchemaRegisterAttribute> schemaAttributes = [];
        foreach (var ctor in constructors)
        {
            var attributes = ctor.GetCustomAttributes(typeof(ISchemaRegisterAttribute), true);
            foreach (var attribute in attributes.Cast<ISchemaRegisterAttribute>())
            {
                schemaAttributes.Add(attribute);
                attribute.Register<T>();
            }
        }

        if (schemaAttributes.Count == 0)
            throw new Exception($"No ModelAttribute found for DbSchema {typeof(T).Name}. " +
                                "Please add a ModelAttribute to the constructor of the DbSchema class.");
        foreach (var attribute in schemaAttributes)
        {
            attribute.RegisterConnections<T>();
        }

        return true;
    }

    public static void Configure<T>() where T : DbSchema, new()
    {
        if (!TryRegister<T>())
            throw new Exception($"DbSchema {typeof(T).Name} is already registered.");
        DbSchema.Verify<T>();
    }

    public static PostgreSqlContainer Mock<T>() where T : DbSchema, new()
    {
        TryRegister<T>();
        var container = DbMocker.Mock<T>();
        DbSchema.Verify<T>();
        return container;
    }


    //
    // public static void Reset()
    // {
    //     ModelCache.Reset();
    // }

    public static void AddConnection(string server, string database, string user, string password)
    {
        var connectionString = $"Server={server};Database={database};User Id={user};Password={password}";
        if (!ConnectionStrings.TryAdd(database, connectionString))
            throw new Exception($"Connection string for database {database} already exists.");
    }
    
    public static void MockConnection(string database,ushort port)
    {
        var connectionString = $"Server=localhost;Port={port};Database={database};User Id=mocking;Password=mocking";
        ConnectionStrings[database] = connectionString;
    }


    internal static string GetConnectionString(string database)
    {
        if (ConnectionStrings.TryGetValue(database, out var connectionString))
            return connectionString;
        throw new Exception($"Connection string for database {database} not found. " +
                            "Please add a connection string using AddConnection method.");
    }
}