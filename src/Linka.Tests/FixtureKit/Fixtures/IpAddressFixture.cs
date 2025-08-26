using Webamoki.Linka;
using Webamoki.Linka.Testing;

namespace Tests.FixtureKit.Fixtures;

public class IpAddressFixture : Fixture<UserDbSchema>, IFixture
{
    public override void Inject()
    {
        var model = new IpAddressModel(
            "30120320SU",
            "127.0.0.1"
            );
        using var db = new DbService<UserDbSchema>();
        db.Insert(model);
    }
}