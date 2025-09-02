using NUnit.Framework;
using Tests.FixtureKit;
using Tests.FixtureKit.Fixtures;
using Webamoki.Linka;
using Webamoki.Linka.Testing;
using Webamoki.Utils.Testing;

namespace Tests.IntegrationTesting;

[Fixtures<UserModelFixture>]
[Fixtures<IpAddressFixture>]
public class UpdateTest
{
    [Test]
    public void Update_UserModel_ReturnsExpected()
    {
        using var db = new DbService<UserSchema>();
        db.Update<UserModel>(u => u.ID == "AAAAAAAAAA")
            .Set(u => u.Name, "John2")
            .Set(u => u.Rank, UserModel.RankEnum.Admin)
            .Save();

        var model = db.Get<UserModel>(u => u.ID == "AAAAAAAAAA");
        Ensure.Equal("John2", model.Name.Value());
        Ensure.Equal(UserModel.RankEnum.Admin, model.Rank.Value());
    }

    [Test]
    public void UpdateWhenCached_UserModel_ReturnsExpected()
    {
        using var db = new DbService<UserSchema>();
        _ = db.Get<UserModel>(u => u.ID == "AAAAAAAAAA");
        db.Update<UserModel>(u => u.ID == "AAAAAAAAAA")
            .Set(u => u.Name, "John2")
            .Set(u => u.Rank, UserModel.RankEnum.Admin)
            .Save();

        var model = db.Get<UserModel>(u => u.ID == "AAAAAAAAAA");
        Ensure.Equal("John2", model.Name.Value());
        Ensure.Equal(UserModel.RankEnum.Admin, model.Rank.Value());
    }

}
