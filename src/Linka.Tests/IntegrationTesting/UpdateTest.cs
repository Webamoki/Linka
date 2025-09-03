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

        using var db2 = new DbService<UserSchema>();
        var model = db2.Get<UserModel>(u => u.ID == "AAAAAAAAAA");
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

    [Test]
    public void DirectUpdate_UserModel_ReturnsExpected()
    {
        using var db = new DbService<UserSchema>();
        var model = db.Get<UserModel>(u => u.ID == "AAAAAAAAAA");
        model.Name.Value("John2");
        model.Rank.Value(UserModel.RankEnum.Admin);

        var model2 = db.Get<UserModel>(u => u.ID == "BBBBBBBBBB");
        model2.Name.Value("Alice2");
        model2.Rank.Value(UserModel.RankEnum.User);

        db.SaveChanges();

        using var db2 = new DbService<UserSchema>();
        model = db2.Get<UserModel>(u => u.ID == "AAAAAAAAAA");
        Ensure.Equal(model.Name.Value(), "John2");
        Ensure.Equal(model.Rank.Value(), UserModel.RankEnum.Admin);

        model2 = db2.Get<UserModel>(u => u.ID == "BBBBBBBBBB");
        Ensure.Equal(model2.Name.Value(), "Alice2");
        Ensure.Equal(model2.Rank.Value(), UserModel.RankEnum.User);
    }

    [Test]
    public void DirectUpdatePrimaryKey_UserModel_ReturnsExpected()
    {
        using var db = new DbService<UserSchema>();
        var model = db.Get<UserModel>(u => u.ID == "AAAAAAAAAA");
        model.ID.Value("ZZZZZZZZZZ");
        Ensure.Throws<Exception>(() => db.SaveChanges());
    }

}
