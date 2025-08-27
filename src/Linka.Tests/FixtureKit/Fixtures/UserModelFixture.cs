using Webamoki.Linka;
using Webamoki.Linka.Testing;

namespace Tests.FixtureKit.Fixtures;

public class UserModelFixture : Fixture<UserSchema>, IFixture
{
    public override void Inject()
    {
        var model = new UserModel(
            "John",
            "johndoe@example.com",
            "1234567890",
            UserModel.RankEnum.User,
            "password",
            null,
            true,
            false,
            100
            );
        model.ID.Value("AAAAAAAAAA");
        using var db = new DbService<UserSchema>();
        db.Insert(model);
    }
}