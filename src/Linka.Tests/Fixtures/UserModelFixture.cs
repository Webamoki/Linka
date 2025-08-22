using Tests.Models;
using Webamoki.Linka;
using Webamoki.Linka.TestUtils;

namespace Tests.Fixtures;

public class UserModelFixture : Fixture<UserDbSchema>, IFixture
{
    public override void Inject()
    {
        var model = new UserModel(
            "Fred",
            "fred@example.com",
            "1234567890",
            UserModel.UserRank.User,
            "password",
            null,
            true,
            false,
            100
            );
        model.ID.Value("30120320SU");
        DbService<UserDbSchema> db = new();
        db.Insert(model);
    }
}