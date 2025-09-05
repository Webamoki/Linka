using Webamoki.Linka.Queries;
using Webamoki.Utils;

namespace Webamoki.Linka.SchemaSystem;

public static class SchemaCheck
{
    /// <summary>
    ///     Verifies that the database schema matches the expected Schema definition.
    /// </summary>
    /// <typeparam name="T">The Schema type to verify</typeparam>
    public static void Check<T>() where T : Schema, new()
    {
        var schema = Schema.Get<T>();
        Logging.WriteLog($"Verifying Schema {typeof(T).Name} for schema {schema.Name}");
        CheckEnums<T>(schema);
        Logging.WriteLog($"Schema {typeof(T).Name} verification completed successfully");
    }

    /// <summary>
    ///     Verifies that all enums defined in the schema exist in the database with correct values.
    /// </summary>
    /// <typeparam name="T">The Schema type</typeparam>
    /// <param name="schema">The schema instance</param>
    private static void CheckEnums<T>(Schema schema) where T : Schema, new()
    {
        if (schema.Enums.Count == 0)
        {
            Logging.WriteLog("No enums defined in schema - skipping enum verification");
            return;
        }

        Logging.WriteLog($"Verifying {schema.Enums.Count} enums in schema {schema.Name}");

        const string sqlQuery = """
                                SELECT 
                                    t.typname AS EnumName,
                                    string_agg(e.enumlabel, ',' ORDER BY e.enumsortorder) AS EnumValues
                                FROM pg_type t
                                JOIN pg_enum e ON t.oid = e.enumtypid
                                JOIN pg_namespace n ON t.typnamespace = n.oid
                                WHERE n.nspname = ?
                                  AND t.typtype = 'e'
                                GROUP BY t.typname
                                ORDER BY t.typname;
                                """;

        var query = new Query(sqlQuery);
        query.AddValue(schema.Name);

        using var dbService = new DbService<T>();
        var reader = query.Execute(dbService);

        // Create a set of expected enum names for tracking
        var expectedEnums = new Dictionary<string, string>();
        foreach (var (_, (enumName, enumType)) in schema.Enums) expectedEnums[enumName] = enumType[6..^1].Replace("'", "");

        // Verify each enum found in the database
        var foundEnums = new HashSet<string>();
        while (reader.Read())
        {
            var enumName = reader["EnumName"].ToString()!;
            var enumValues = reader["EnumValues"].ToString()!;

            if (!expectedEnums.TryGetValue(enumName, out var expectedType))
                throw new Exception($"Unexpected enum '{enumName}' found in database schema '{schema.Name}'. " +
                                    "This enum is not defined in the Schema class.");

            Assert(expectedType, enumValues, $"Enum {enumName} in schema {schema.Name} has unexpected values. Expected: {expectedType}, got: {enumValues}.");

            _ = foundEnums.Add(enumName);
            Logging.WriteLog($"--- Verifying enum {enumName} in schema {schema.Name}");
        }

        if (expectedEnums.Count != foundEnums.Count)
        {
            var missingEnums = expectedEnums.Keys.Except(foundEnums);
            throw new Exception($"Enums {string.Join(", ", missingEnums)} are not found in database schema '{schema.Name}'. " +
                                "Please add these enums to the database.");
        }
    }

    private static void Assert(object expected, object result, string message)
    {
        if (!Equals(expected, result))
            throw new Exception($"Assertion failed: {expected} != {result}");
        if (!Equals(expected, result))
            throw new Exception(message);
    }
}