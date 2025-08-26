
namespace Webamoki.Linka.Testing;

public interface IFixture
{
    void Inject();
    DbSchema Schema();
    void TryCompile();
}

public abstract class Fixture<T> : IFixture where T : DbSchema, new()
{
    public abstract void Inject();

    public DbSchema Schema() => DbSchema.Get<T>();
    
    public void TryCompile()
    {
        Linka.TryCompile<T>();
    }
}