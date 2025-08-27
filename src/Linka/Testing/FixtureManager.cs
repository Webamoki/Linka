using Testcontainers.PostgreSql;

namespace Webamoki.Linka.Testing;

internal class FixtureManager
{
    private static Dictionary<string,FixtureManager> Instances = [];
    private readonly List<IFixture> _fixtures = [];
    private readonly HashSet<String> _fixtureNames = [];
    private bool _isComplete;
    
    private FixtureManager(){}
    public static FixtureManager Get(string testClass)
    {
        if (!Instances.TryGetValue(testClass, out var instance))
        {
            instance = new FixtureManager();
            Instances.Add(testClass, instance);
        }
        return instance;
    }
    private static readonly Dictionary<string,object> DatabaseLocks = [];
    private List<PostgreSqlContainer> _containers = [];
    public int Count => _fixtures.Count;
    public bool IsComplete() => _isComplete;
    public void Complete() => _isComplete = true;
    
    public void Add(IFixture fixture)
    {
        if (!_fixtureNames.Add(fixture.GetType().Name))
        {
            throw new ArgumentException("Cannot add the same fixture twice.");
        }
        _fixtures.Add(fixture);
    }

    
    public bool IsLast<T>() where T : IFixture
    {
        return _fixtures.Last() is T;
    }

    public void Load()
    {
        HashSet<string> loaded = [];
        foreach (var fixture in _fixtures)
        {
            fixture.TryCompile();
            var schemaName = fixture.Schema().Name;
            if (loaded.Add(schemaName))
            {
                DatabaseLocks.TryAdd(schemaName, new object());
                Monitor.Enter(DatabaseLocks[schemaName]);
                _containers.Add(fixture.Schema().SchemaGeneric!.Mock());
            }
            fixture.Inject();
        }
    }
    
    public void Dispose()
    {
        foreach (var container in _containers)
        {
            container.DisposeAsync().AsTask().Wait();
        }
        _containers.Clear();
        foreach (var fixture in _fixtures)
        {
            var schemaName = fixture.Schema().Name;
            if (!DatabaseLocks.TryGetValue(schemaName, out var lockObject)) continue;
            Monitor.Exit(lockObject);
            DatabaseLocks.Remove(schemaName);
        }
    }
}