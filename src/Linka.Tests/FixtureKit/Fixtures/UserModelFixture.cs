using Webamoki.Linka;
using Webamoki.Linka.Testing;

namespace Tests.FixtureKit.Fixtures;

public class UserModelFixture : Fixture<UserSchema>, IFixture
{
    public override void Inject()
    {
        // User 1
        var model1 = new UserModel(
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
        model1.ID.Value("AAAAAAAAAA");

        // User 2
        var model2 = new UserModel(
            "Alice",
            "alice@example.com",
            "0987654321",
            UserModel.RankEnum.Admin,
            "securePass123",
            null,
            true,
            true,
            250
        );
        model2.ID.Value("BBBBBBBBBB");

        // User 3
        var model3 = new UserModel(
            "Bob",
            "bob@example.com",
            "5556667777",
            UserModel.RankEnum.Admin,
            "bobPass!",
            null,
            false,
            false,
            75
        );
        model3.ID.Value("CCCCCCCCCC");

        // User 4
        var model4 = new UserModel(
            "Eve",
            "eve@example.com",
            "4443332222",
            UserModel.RankEnum.User,
            "eveSecure",
            null,
            true,
            false,
            0
        );
        model4.ID.Value("DDDDDDDDDD");

        // User 5
        var model5 = new UserModel(
            "Charlie",
            "charlie@example.com",
            "2223334444",
            UserModel.RankEnum.User,
            "charliePass",
            null,
            true,
            true,
            500
        );
        model5.ID.Value("EEEEEEEEEE");

        using var db = new DbService<UserSchema>();
        db.Insert(model1);
        db.Insert(model2);
        db.Insert(model3);
        db.Insert(model4);
        db.Insert(model5);
    }

}