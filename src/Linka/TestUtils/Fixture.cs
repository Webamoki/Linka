
namespace Webamoki.Linka.TestUtils;

public interface IFixture
{
    void Inject();
    DbSchema Schema();
    void TryRegister();
}

public abstract class Fixture<T> : IFixture where T : DbSchema, new()
{
    public abstract void Inject();

    public DbSchema Schema() => DbSchema.Get<T>();
    
    public void TryRegister()
    {
        Linka.TryRegister<T>();
    }
}