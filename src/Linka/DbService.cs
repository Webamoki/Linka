using System.Data;
using System.Linq.Expressions;
using System.Text;
using Npgsql;
using Webamoki.Linka.Models;
using Webamoki.Linka.Queries;
using Webamoki.Utils;

namespace Webamoki.Linka;

public interface IDbService
{
    NpgsqlDataReader Execute(string query, List<object> values);
    DatabaseCode ExecuteTransaction(string query, List<object> values);
    T First<T>(Expression<Func<T, bool>> expression) where T : Model, new();
    T? FirstOrNull<T>(Expression<Func<T, bool>> expression) where T : Model, new();
    T Single<T>(Expression<Func<T, bool>> expression) where T : Model, new();
    T? SingleOrNull<T>(Expression<Func<T, bool>> expression) where T : Model, new();
    IncludeQuery<T> Include<T>(Expression<Func<T, object>> expression) where T : Model, new();
    DbSchema Schema { get; }
}

public sealed class DbService<TDbSchema> : IDbService, IDisposable where TDbSchema : DbSchema, new()
{
    private readonly NpgsqlConnection _connection;
    private readonly bool _debug;
    public DbService(bool debug = false)
    {
        var schema = DbSchema.Get<TDbSchema>();
        _connection = new NpgsqlConnection(schema.ConnectionString);
        _debug = debug || Linka.Debug;
    }

    public DbSchema Schema => DbSchema.Get<TDbSchema>();

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

    public T First<T>(Expression<Func<T, bool>> expression) where T : Model, new() =>
        new SingleModelQuery<T>(this, expression).First();

    public T? FirstOrNull<T>(Expression<Func<T, bool>> expression) where T : Model, new() =>
        new SingleModelQuery<T>(this, expression).FirstOrNull();

    public T Single<T>(Expression<Func<T, bool>> expression) where T : Model, new() =>
        new SingleModelQuery<T>(this, expression).Single();

    public T? SingleOrNull<T>(Expression<Func<T, bool>> expression) where T : Model, new() =>
        new SingleModelQuery<T>(this, expression).SingleOrNull();

    public IncludeQuery<T> Include<T>(Expression<Func<T, object>> expression) where T : Model, new() =>
        new(this, expression);


}

public enum DatabaseCode
{
    DuplicateEntry = 23505,
    Success = 99992,
    InvalidModel = 99993,
    InvalidField = 99994
}