using Webamoki.Linka.SchemaSystem;

namespace Webamoki.Linka.Testing;

public interface IFixture
{
    void Inject();
    Schema Schema();
    void TryCompile();
}

public abstract class Fixture<T> : IFixture where T : Schema, new()
{
    public abstract void Inject();

    public Schema Schema() => SchemaSystem.Schema.Get<T>();

    public void TryCompile() => Linka.TryCompile<T>();
}