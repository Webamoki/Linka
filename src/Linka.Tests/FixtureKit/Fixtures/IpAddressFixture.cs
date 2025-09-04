using Webamoki.Linka;
using Webamoki.Linka.Testing;

namespace Tests.FixtureKit.Fixtures;

public class IpAddressFixture : Fixture<UserSchema>, IFixture
{
    public override void Inject()
    {
        var model = new IpAddressModel(
            "AAAAAAAAAA",
            "127.0.0.1"
        );
        using var db = new DbService<UserSchema>();
        db.Insert(model);
        _ = db.SaveChanges();
    }
}