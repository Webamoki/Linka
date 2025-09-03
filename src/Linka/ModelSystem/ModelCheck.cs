using Webamoki.Linka.Fields;
using Webamoki.Linka.Queries;
using Webamoki.Linka.SchemaSystem;
using Webamoki.Utils;

namespace Webamoki.Linka.ModelSystem;

public static class ModelCheck
{
    public static void Check<T>(Type modelType) where T : Schema, new()
    {
        CheckFields<T>(modelType);
        CheckRelations<T>(modelType);
        CheckFulltext<T>(modelType);
    }


    private static void CheckFields<T>(Type modelType) where T : Schema, new()
    {
        var schema = Schema.Get<T>();
        const string sqlQuery = """
                                SELECT
                                    a.attname AS ColumnName,
                                    CASE
                                        WHEN ct.contype = 'p' THEN true ELSE false
                                    END AS IsPrimary,
                                    COALESCE(
                                    ( (ct.contype = 'p' AND array_length(ct.conkey, 1) = 1)
                                        OR (ct.contype = 'u' AND array_length(ct.conkey, 1) = 1)
                                      ),
                                      FALSE
                                    ) AS IsUnique,
                                    NOT a.attnotnull AS IsNullable,
                                    UPPER(CASE
                                        WHEN t.typname = 'bool' THEN 'BOOLEAN'
                                        WHEN t.typname = 'int2' THEN 'SMALLINT'
                                        WHEN t.typname = 'int4' THEN 'INT'
                                        WHEN t.typname = 'int8' THEN 'BIGINT'
                                        WHEN t.typname = 'varchar' THEN 'varchar(' || (a.atttypmod - 4) || ')'
                                        WHEN t.typname = 'timestamp' THEN 'timestamp(' || (a.atttypmod) || ')'
                                        WHEN t.typname = 'bpchar' THEN 'char(' || (a.atttypmod - 4) || ')'
                                        WHEN t.typname = 'numeric' THEN 'numeric(' || ((a.atttypmod - 4) >> 16) || ',' || ((a.atttypmod - 4) & 65535) || ')'
                                        ELSE t.typname
                                    END) AS ColumnType
                                FROM pg_attribute a
                                JOIN pg_class c ON a.attrelid = c.oid
                                JOIN pg_namespace n ON c.relnamespace = n.oid
                                LEFT JOIN pg_type t ON a.atttypid = t.oid
                                LEFT JOIN pg_constraint ct
                                    ON ct.conrelid = c.oid
                                    AND a.attnum = ANY (ct.conkey)
                                    AND ct.contype IN ('p', 'u')
                                WHERE a.attnum > 0
                                  AND NOT a.attisdropped
                                  AND c.relname = ?
                                  AND n.nspname = ?
                                ORDER BY a.attnum;
                                """;

        var query = new Query(sqlQuery);
        var tableName = ModelRegistry.Get(modelType).TableName;
        Logging.WriteLog($"Verifying model {tableName} in schema {schema.Name}");
        query.AddValue(tableName);
        query.AddValue(schema.Name);

        using var dbService = new DbService<T>();
        var reader = query.Execute(dbService);
        HashSet<string> fieldNames = [];
        foreach (var (fieldName, _) in ModelRegistry.Get(modelType).Fields)
            fieldNames.Add(fieldName);

        var fields = ModelRegistry.Get(modelType).Fields;
        while (reader.Read())
        {
            if (fieldNames.Count == 0) throw new Exception("Table has more fields than expected.");
            var fieldName = reader.GetString(0);
            Logging.WriteLog($"--- Verifying field {fieldName} in model {tableName}");
            fieldNames.Remove(fieldName);
            if (!fields.TryGetValue(fieldName, out var field)) throw new Exception($"Field {fieldName} does not exist in model: {tableName}.");
            Assert(field.IsPrimary, reader["IsPrimary"], $"Column {fieldName} in table {tableName} needs to be a primary key.");
            Assert(field.IsUnique, reader["IsUnique"], $"Column {fieldName} in table {tableName} needs to be unique.");
            Assert(!field.IsRequired, reader["IsNullable"], $"Column {fieldName} in table {tableName} needs to be nullable.");
            var expectedType = reader["ColumnType"];
            var sqlType = field.SQLType;
            if (field is IEnumDbField enumField)
            {
                sqlType = $"{enumField.GetSchemaEnumName<T>()}";
                sqlType = sqlType.ToUpper();
            }
            Assert(sqlType, expectedType, $"Column {fieldName} in table {tableName} has unexpected type. Expected: {field.SQLType}, got: {expectedType}.");
        }
        if (fieldNames.Count > 0)
            throw new Exception($"Fields {string.Join(", ", fieldNames)} do not exist in table: {tableName}.");
    }

    private static void CheckFulltext<T>(Type modelType) where T : Schema, new()
    {
        var schema = Schema.Get<T>();
        const string sqlQuery = """
                                SELECT
                                    pg_get_indexdef(i.oid) AS Definition
                                FROM
                                    pg_class t
                                JOIN
                                    pg_namespace n ON t.relnamespace = n.oid
                                JOIN
                                    pg_index ix ON t.oid = ix.indrelid
                                JOIN
                                    pg_class i ON i.oid = ix.indexrelid
                                JOIN
                                    pg_am am ON i.relam = am.oid
                                WHERE
                                    t.relname = ?
                                    AND n.nspname = ?
                                    AND i.relname = ?
                                    AND am.amname = 'gin'
                                    AND ix.indisvalid = true
                                
                                """;

        var query = new Query(sqlQuery);
        var tableName = ModelRegistry.Get(modelType).TableName;
        Logging.WriteLog($"Verifying model {tableName} full text columns in schema {schema.Name}");
        query.AddValue(tableName);
        query.AddValue(schema.Name);
        query.AddValue($"FT_{tableName}");

        using var dbService = new DbService<T>();
        var reader = query.Execute(dbService);
        var definition = "";
        var fields = "";
        foreach (var (fieldName, field) in ModelRegistry.Get(modelType).Fields)
        {
            if (field.Search == 0) continue;
            if (fields != "") fields += ", ";
            var definitionPart = $"""("{fieldName}")::text""";
            definition = definition == "" ? $"{definitionPart}" : $"(({definition} || ' '::text) || {definitionPart})";
        }

        if (reader.Read())
        {
            var databaseDefinition = reader.GetString(0);
            definition = $"""CREATE INDEX "FT_{tableName}" ON "{schema.Name}"."{tableName}" USING gin (to_tsvector('english'::regconfig, {definition}))""";
            Assert(definition, databaseDefinition, $"FullText index for model {tableName} does index all fields {fields}.");
            return;
        }
        throw new Exception($"FullText index for model {tableName} does not exist in schema {schema.Name}.");
    }

    private static void CheckRelations<T>(Type modelType) where T : Schema, new()
    {
        var schema = Schema.Get<T>();
        const string sqlQuery = """
                                SELECT
                                    tc.constraint_name,
                                    kcu.column_name AS "Column",
                                    ccu.table_name AS "TargetTable",
                                    ccu.column_name AS "TargetColumn",
                                    rc.update_rule AS "OnUpdate",
                                    rc.delete_rule AS "OnDelete"
                                FROM
                                    information_schema.table_constraints AS tc
                                JOIN information_schema.key_column_usage AS kcu
                                    ON tc.constraint_name = kcu.constraint_name
                                   AND tc.constraint_schema = kcu.constraint_schema
                                JOIN information_schema.constraint_column_usage AS ccu
                                    ON ccu.constraint_name = tc.constraint_name
                                   AND ccu.constraint_schema = tc.constraint_schema
                                JOIN information_schema.referential_constraints AS rc
                                    ON rc.constraint_name = tc.constraint_name
                                   AND rc.constraint_schema = tc.constraint_schema
                                WHERE
                                    tc.constraint_type = 'FOREIGN KEY'
                                    AND tc.table_schema = ?
                                    AND tc.table_name = ?;
                                
                                """;

        var query = new Query(sqlQuery);
        var tableName = ModelRegistry.Get(modelType).TableName;
        Logging.WriteLog($"Verifying model {tableName} relations in database {schema.Name}");
        query.AddValue(schema.Name);
        query.AddValue(tableName);

        using var dbService = new DbService<T>();
        var reader = query.Execute(dbService);
        HashSet<string> navigationNames = [];

        foreach (var (navigationName, _) in ModelRegistry.Get(modelType).Navigations)
            navigationNames.Add(navigationName);

        var navInfos = ModelRegistry.Get(modelType).Navigations;
        while (reader.Read())
        {
            if (navigationNames.Count == 0) throw new Exception("Table has more constraints than expected.");
            var navigationName = reader.GetString(0);
            var constraintPrefix = Appendix.ConstraintPrefix(tableName);
            if (!navigationName.StartsWith(constraintPrefix))
                throw new Exception($"Constraint {navigationName} does not match expected prefix {constraintPrefix} for table: {tableName}.");
            navigationName = navigationName[constraintPrefix.Length..];
            Logging.WriteLog($"Verifying navigation {navigationName} in model: {tableName}");
            navigationNames.Remove(navigationName);
            var navInfo = navInfos[navigationName];
            Assert(navInfo.Field, reader["Column"], $"Navigation {navigationName} in model {tableName} does not match expected field: {navInfo.Field}.");
            Assert(navInfo.TargetModelInfo.TableName, reader["TargetTable"], $"Navigation {navigationName} in model {tableName} does not match expected target table: {navInfo.TargetModelInfo.TableName}.");
            Assert(navInfo.TargetModelInfo.PrimaryField!.Name, reader["TargetColumn"], $"Navigation {navigationName} in model {tableName} does not match expected target column: {navInfo.TargetModelInfo.PrimaryField!.Name}.");
            Assert(navInfo.Constraint.ToSqlString(), reader["OnDelete"], $"Navigation {navigationName} in model {tableName} does not match expected OnDelete action: {navInfo.Constraint.ToSqlString()}.");
            Assert("RESTRICT", reader["OnUpdate"], $"Navigation {navigationName} in model {tableName} does not match expected OnUpdate action: RESTRICT.");
        }
        if (navigationNames.Count > 0)
            throw new Exception($"Navigations {string.Join(", ", navigationNames)} do not exist in table: {tableName}.");
    }

    private static void Assert(object expected, object result, string message)
    {
        if (!Equals(expected, result))
            throw new Exception($"Assertion failed: {expected} != {result}");
        if (!Equals(expected, result))
            throw new Exception(message);
    }
}