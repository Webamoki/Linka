using Tests.Models;
using Webamoki.Linka;
using Webamoki.Linka.TestUtils;

namespace Tests.Fixtures;

public class UserModelFixture : Fixture<UserModel>, IFixture
{
    public override void Inject()
    {
        var model = new UserModel(
            "Fred",
            "fred@example.com",
            "1234567890",
            UserModel.UserRank.User,
            "password",
            "cartToken",
            true,
            false,
            100
            );
        DbService<UserDbSchema> db = new();
        db.Insert(model);
    }
}