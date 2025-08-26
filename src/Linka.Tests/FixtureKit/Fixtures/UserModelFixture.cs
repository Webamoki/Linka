using Webamoki.Linka;
using Webamoki.Linka.Testing;

namespace Tests.FixtureKit.Fixtures;

public class UserModelFixture : Fixture<UserDbSchema>, IFixture
{
    public override void Inject()
    {
        var model = new UserModel(
            "Fred",
            "fred@example.com",
            "1234567890",
            UserModel.RankEnum.User,
            "password",
            null,
            true,
            false,
            100
            );
        model.ID.Value("30120320SU");
        using var db = new DbService<UserDbSchema>();
        db.Insert(model);
    }
}