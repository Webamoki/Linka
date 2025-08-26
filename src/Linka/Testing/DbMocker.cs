using Testcontainers.PostgreSql;
using Webamoki.Linka.Fields;
using Webamoki.Linka.ModelSystem;
using Webamoki.Linka.SchemaSystem;

namespace Webamoki.Linka.Testing;

public static class DbMocker
{
    public static PostgreSqlContainer Mock<T>() where T : Schema, new()
    {
        var schema = Schema.Get<T>();
        // create docker container
        var databaseName = $"mock_{schema.Name}";
        var container = new PostgreSqlBuilder()
            .WithDatabase(databaseName)
            .WithUsername("mocking")
            .WithPassword("mocking")
            .WithPortBinding(5432,true)
            .Build();
        container.StartAsync().Wait();
        Linka.ForceConnection(
            "localhost",
            databaseName,
            "mocking",
            "mocking",
            container.GetMappedPublicPort(5432),
            true
        );
        Linka.Register<T>(databaseName);

        using var db = new DbService<T>();
        var script = CreateSchemaQuery<T>(schema);
        db.ExecuteScript(script);
        return container;
    }

    private static string CreateSchemaQuery<T>(Schema schema) where T : Schema, new()
    {
        var enumQueries = "";
        var tableQueries = "";
        var constraintQueries = "";


        foreach (var modelType in schema.Models)
        {
            var info = ModelRegistry.Get(modelType);
            tableQueries += CreateTableQuery<T>(info) + "\n";
            constraintQueries += CreateConstraintQuery(info) + "\n";
        }

        foreach (var (_, (name, enumString)) in schema.Enums)
        {
            enumQueries += $"CREATE TYPE \"{name}\" AS {enumString};";
        }
        
        return $"""
                BEGIN;
                CREATE SCHEMA IF NOT EXISTS "{schema.Name}";
                
                SET search_path TO "{schema.Name}";
                
                {enumQueries}
                {tableQueries}
                {constraintQueries}
                COMMIT;
                """;
    }


    private static string CreateTableQuery<T>(IModelInfo info) where T : Schema, new()
    {
        var columns = "";
        var keys = "";
        List<string> fullTextColumns = [];
        foreach (var (fieldName, field) in info.Fields)
        {
            var nullable = field.IsRequired ? " NOT NULL" : "";
            var sqlType = field.SQLType;
            if (sqlType.StartsWith("ENUM"))
            {
                var enumField = (IEnumDbField)field;
                var enumType = $"\"{enumField.GetSchemaEnumName<T>()}\"";
                sqlType = enumType;
            }

            columns += $"\t\"{fieldName}\" {sqlType}{nullable},\n";

            if (field is { IsPrimary: false, IsUnique: true })
            {
                keys += $"ALTER TABLE \"{info.TableName}\" ADD CONSTRAINT \"{Appendix.UniqueKey(fieldName)}\" UNIQUE (\"{fieldName}\");\n";
            }

            if (field.Search != 0)
            {
                fullTextColumns.Add(fieldName);
            }
        }

        var fullText = "";
        if (fullTextColumns.Count > 0)
        {
            fullText = $"""
                        CREATE INDEX "FT_{info.TableName}"
                        ON "{info.TableName}"
                        USING GIN (
                          to_tsvector('english', {string.Join(" || ' ' || ", fullTextColumns.Select(c => $"\"{c}\""))})
                        );
                        """;
        }

        var query = $"""
                      CREATE TABLE "{info.TableName}" (
                        {columns}
                        PRIMARY KEY ({string.Join(", ", info.PrimaryFields.Select(c => $"\"{c.Key}\""))})
                      );
                      {keys}
                      {fullText}
                      """;

        return query;
    }

    private static string CreateConstraintQuery(IModelInfo info)
    {
        var tableName = info.TableName;
        var constraints = "";
        foreach (var (navigationName, navigation) in info.Navigations)
        {
            constraints += $"""
                            ALTER TABLE "{tableName}"
                            ADD CONSTRAINT "{Appendix.Constraint(tableName, navigationName)}"
                            FOREIGN KEY ("{navigation.Field}")
                            REFERENCES "{navigation.TargetModelInfo.TableName}" ("{navigation.TargetField}")
                            ON DELETE {navigation.Constraint.ToSqlString()} ON UPDATE RESTRICT;
                            """;
        }

        return constraints;
    }
}