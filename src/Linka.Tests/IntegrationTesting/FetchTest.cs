using NUnit.Framework;
using Tests.FixtureKit;
using Tests.FixtureKit.Fixtures;
using Webamoki.Linka;
using Webamoki.Linka.Testing;
using Webamoki.Utils;
using Webamoki.Utils.Testing;

namespace Tests.IntegrationTesting;

[Fixtures<UserModelFixture>]
[Fixtures<IpAddressFixture>]
public class FetchTest
{
    [Test]
    public void Get_UserModel_ReturnsExpected()
    {
        using var db = new DbService<UserSchema>();
        var model = db.Get<UserModel>(u => u.ID == "AAAAAAAAAA");
        Ensure.Equal("John", model.Name.Value());
        Ensure.Equal("johndoe@example.com", model.Email.Value());
        Ensure.Equal("1234567890", model.Phone.Value());
        Ensure.Equal(UserModel.RankEnum.User, model.Rank.Value());
        Ensure.Equal("password", model.Password.Value());
        Ensure.Equal(null, model.CartToken.Value());
        Ensure.True(model.Verified.Value());
        Ensure.False(model.Login.Value());
        Ensure.Equal(100, model.Credit.Value());

        var ip = db.Get<IpAddressModel>(i => i.IP == "127.0.0.1");
        Ensure.Equal("AAAAAAAAAA", ip.UserID.Value());
        Ensure.Equal("127.0.0.1", ip.IP.Value());
    }

    [Test]
    public void GetOrNull_UserModel_ReturnsExpected()
    {
        using var db = new DbService<UserSchema>();
        var model = db.GetOrNull<UserModel>(u => u.ID == "AAAAAAAAAA");
        Ensure.NotNull(model);
        if (model != null) Ensure.Equal("John", model.Name.Value());
        model = db.GetOrNull<UserModel>(u => u.ID == "ZZZZZZZZZZ");
        Ensure.Null(model);
    }

    [Test]
    public void Include_IPAddress_ReturnsExpected()
    {
        Logging.Enable();
        Logging.Hold("PostgreSQL: Query");
        using var db = new DbService<UserSchema>(true);
        var user = db.Include<UserModel>(u => u.IpAddresses).Get(u => u.ID == "AAAAAAAAAA");
        var ip = user.IpAddresses[0];
        Ensure.Count(user.IpAddresses, 1);
        Ensure.Equal("AAAAAAAAAA", ip.UserID.Value());
        Ensure.Equal("127.0.0.1", ip.IP.Value());
        Ensure.Equal("John", user.Name.Value());
        Ensure.Equal("johndoe@example.com", user.Email.Value());
        var logs = Logging.GetHeldLogs("PostgreSQL: Query");
        Ensure.Count(logs, 2);
        Logging.ClearBuffer();
        // Cache Check
        Ensure.Count(Logging.GetHeldLogs("PostgreSQL: Query"), 0);
        _ = db.Get<UserModel>(u => u.ID == "AAAAAAAAAA");
        _ = db.Get<IpAddressModel>(i => i.UserID == "AAAAAAAAAA");
        Ensure.Count(Logging.GetHeldLogs("PostgreSQL: Query"), 0);
    }

    [Test]
    public void GetMany_UserModel_ReturnsExpected()
    {
        using var db = new DbService<UserSchema>();
        var models = db.GetMany<UserModel>(u => u.Rank == UserModel.RankEnum.Admin).Load();
        Ensure.Count(models, 2);
        Ensure.Equal("Alice", models[0].Name.Value());
        Ensure.Equal("Bob", models[1].Name.Value());
        Ensure.Equal(3, db.GetMany<UserModel>(u => u.Rank == UserModel.RankEnum.User).Count());
    }
}