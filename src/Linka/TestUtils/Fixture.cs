using Webamoki.Linka.Models;

namespace Webamoki.Linka.Utils.Testing;

public interface IFixture
{
    void Inject();
    DbSchema Schema();
}

public abstract class Fixture<T> : IFixture
    where T : Model
{
    public abstract void Inject();

    public DbSchema Schema() =>
        DbSchema.GetWithModel<T>();
}