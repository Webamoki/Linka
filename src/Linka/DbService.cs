using System.Data;
using System.Linq.Expressions;
using System.Text;
using Npgsql;
using Webamoki.Linka.Expressions;
using Webamoki.Linka.Fields;
using Webamoki.Linka.ModelSystem;
using Webamoki.Linka.Queries;
using Webamoki.Linka.SchemaSystem;
using Webamoki.Utils;

namespace Webamoki.Linka;

internal interface IDbService
{
    NpgsqlDataReader Execute(string query, List<object> values);
    DatabaseCode ExecuteTransaction(string query, List<object> values);
    Schema Schema { get; }
    public void AddModelToCache<T>(T model) where T : Model;
}

public sealed class DbService<TSchema>(bool debug = false) : IDbService, IDisposable
    where TSchema : Schema, new()
{
    private readonly NpgsqlConnection _connection = new(Linka.ConnectionString<TSchema>());
    private readonly bool _debug = debug || Linka.Debug;
    private Dictionary<Type, IModelCache> _caches = [];

    public Schema Schema => Schema.Get<TSchema>();

    public DatabaseCode ExecuteTransaction(string query, List<object> values)
    {
        var result = DatabaseCode.Success;
        _connection.Open();
        if (_debug)
        {
            Logging.WriteLog($"Executing transaction: {query}");
            Logging.WriteLog($"With Params: {string.Join(", ", values.Select(value => value))}");
        }

        try
        {
            query = $"""SET search_path TO "{Schema.Name}";{query}""";
            using var transaction = _connection.BeginTransaction();
            using var command = CreateCommand(query, values);
            command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (NpgsqlException ex)
        {
            try
            {
                _connection.BeginTransaction().Rollback();
            }
            catch (Exception transactionEx)
            {
                if (_debug)
                {
                    Logging.WriteLog($"Transaction Rollback Error: {transactionEx.Message}");
                }
            }

            result = ProcessError(ex);
        }
        finally
        {
            _connection.Close();
        }

        return result;
    }

    internal DatabaseCode ExecuteScript(string script)
    {
        var result = DatabaseCode.Success;

        try
        {
            _connection.Open();
            if (_debug)
            {
                Logging.WriteLog($"Executing full SQL script {script}");
            }

            using var command = CreateCommand(script, []);
            command.ExecuteNonQuery();
        }
        catch (NpgsqlException ex)
        {
            if (_debug)
            {
                Logging.WriteLog($"SQL Script Execution Error: {ex.Message}");
            }

            result = ProcessError(ex);
        }
        finally
        {
            _connection.Close();
        }

        return result;
    }

    public NpgsqlDataReader Execute(string query, List<object> values)
    {
        if (_debug)
        {
            Logging.WriteLog($"Executing query: {query}");
            Logging.WriteLog($"With Params: {string.Join(", ", values.Select(value => value))}");
        }

        _connection.Open();
        query = $"""SET search_path TO "{Schema.Name}";{query}""";
        using var cmd = CreateCommand(query, values);

        return cmd.ExecuteReader(CommandBehavior.CloseConnection);
    }

    private DatabaseCode ProcessError(NpgsqlException ex)
    {
        if (_debug)
        {
            Logging.WriteLog($"Transaction Error: {ex.Message}");
        }

        if (!int.TryParse(ex.SqlState, out var errorCode))
        {
            throw new Exception($"Error Code: {ex.Message}");
        }

        foreach (DatabaseCode code in Enum.GetValues(typeof(DatabaseCode)))
        {
            if ((int)code == errorCode)
                return code;
        }
        throw ex;
    }

    private NpgsqlCommand CreateCommand(string query, List<object> values, NpgsqlTransaction? transaction = null)
    {
        var paramIndex = 0;
        var result = new StringBuilder();

        foreach (var c in query)
        {
            if (c == '?') result.Append($"@p{paramIndex++}");
            else result.Append(c);
        }

        query = result.ToString();
        var cmd = transaction == null
            ? new NpgsqlCommand(query, _connection)
            : new NpgsqlCommand(query, _connection, transaction);
        paramIndex = 0;
        foreach (var value in values)
            cmd.Parameters.AddWithValue($"@p{paramIndex++}", value);
        return cmd;
    }


    public void Dispose() { _connection.Dispose(); }

    public T Get<T>(Expression<Func<T, bool>> expression) where T : Model, new() =>
        new GetExpression<T>(this, expression).Get();

    public T? GetOrNull<T>(Expression<Func<T, bool>> expression) where T : Model, new() =>
        new GetExpression<T>(this, expression).GetOrNull();
    
    public List<T> GetMany<T>(Expression<Func<T, bool>> expression) where T : Model, new() =>
        new GetExpression<T>(this, expression).GetMany();
    
    public IncludeExpression<T> Include<T>(Expression<Func<T, object>> expression) where T : Model, new() =>
        new(this, expression);
    
    public DatabaseCode Insert<T>(T model) where T : Model
    {
        var info = ModelRegistry.Get<T>();
        var fieldIterator = model.GetFieldIterator();
        // Validate all fields before inserting
        var validationErrors = new List<string>();
        foreach (var (fieldName, field) in fieldIterator.All())
        {
            if (!field.IsSet)
            {
                if (info.Fields[fieldName].IsRequired)
                {
                    validationErrors.Add($"Field '{fieldName}' is required.");
                }
                continue;
            }
            if (!field.IsValid(out var message))
            {
                validationErrors.Add($"Field '{fieldName}': {message}");
            }
        }

        if (validationErrors.Count > 0)
        {
            throw new InvalidOperationException($"Model validation failed: {string.Join(", ", validationErrors)}");
        }

        // Create insert query
        var insertQuery = new InsertQuery(info.TableName);
        var values = new List<object>();

        // Add columns and values for all set fields
        foreach (var (fieldName, field) in fieldIterator.All())
        {
            // Only include fields that have been set and are not auto-generated primary keys
            if (field.IsSet)
            {
                var value = field.ObjectValue();
                if (value is null) continue;
                insertQuery.AddColumn(fieldName);

                if (value is Enum)
                {
                    var enumField = (IEnumDbField)field;
                    insertQuery.AddValueMarker($"'{value}'::\"{enumField.GetSchemaEnumName<TSchema>()}\"");
                }
                else
                {
                    insertQuery.AddValueMarker();
                    values.Add(value);
                }
            }
        }

        if (values.Count == 0)
        {
            throw new InvalidOperationException("No fields have been set for insertion.");
        }

        insertQuery.AddValues(values);
        var code = insertQuery.ExecuteTransaction(this);
        if (code != DatabaseCode.Success)
        {
            throw new InvalidOperationException($"Insert failed with code {code}.");
        }
        
        AddModelToCache(model);
        return code;
    }
    
    // TODO: Remove public modifier
    public void AddModelToCache<T>(T model) where T : Model
    {
        if (!_caches.TryGetValue(typeof(T), out var cache))
        {
            cache = new ModelCache<T>();
            _caches[typeof(T)] = cache;
        }
        cache.Add(model);
    }

}

public enum DatabaseCode
{
    DuplicateEntry = 23505,
    Success = 99992,
    InvalidModel = 99993,
    InvalidField = 99994
}