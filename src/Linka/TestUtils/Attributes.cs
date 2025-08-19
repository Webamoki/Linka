using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Testcontainers.PostgreSql;

namespace Webamoki.Linka.Utils.Testing;

[AttributeUsage(AttributeTargets.Class)]
public class FixturesAttribute : Attribute, ITestAction
{
    private static readonly Dictionary<string,object> DatabaseLocks = [];
    private readonly List<IFixture> _fixtures;
    private readonly DbSchema _schema;
    private readonly object _lock;
    private PostgreSqlContainer? _container;
    public FixturesAttribute(List<IFixture> fixtures)
    {
        if (fixtures.Count == 0)
            throw new ArgumentException("Fixtures list cannot be null or empty.", nameof(fixtures));
        _schema = fixtures[0].Schema();
        foreach (var fixture in fixtures)
        {
            if (fixture.Schema() != _schema)
                throw new ArgumentException("All fixtures must use the same DbSchema.", nameof(fixtures));
        }
        if (!DatabaseLocks.ContainsKey(_schema.DatabaseName))
        {
            DatabaseLocks[_schema.DatabaseName] = new object();
        }
        _lock = DatabaseLocks[_schema.DatabaseName];
        _fixtures = fixtures;
    }
    
    public void BeforeTest(ITest test)
    {
        Monitor.Enter(_lock);
        _container = _schema.SchemaGeneric!.Mock();
        foreach(var fixture in _fixtures)
        {
            fixture.Inject();
        }
    }

    public void AfterTest(ITest test)
    {
        _container!.DisposeAsync().AsTask().Wait();
        Monitor.Exit(_lock);
    }

    public ActionTargets Targets => ActionTargets.Test;
}
[AttributeUsage(AttributeTargets.Class)]
public class RegisterSchemaAttribute<T> : Attribute, ITestAction where T : DbSchema, new()
{
    public void BeforeTest(ITest test)
    {
        Linka.TryRegister<T>();
    }

    public void AfterTest(ITest test)
    {

    }

    public ActionTargets Targets => ActionTargets.Suite;
}
