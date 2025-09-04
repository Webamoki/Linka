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
public class DeleteTest
{
    [Test]
    public void Delete_UserModel_ReturnsExpected()
    {
        using var db = new DbService<UserSchema>();
        var model = db.Get<UserModel>(u => u.ID == "AAAAAAAAAA");
        Ensure.Equal("John", model.Name.Value());
        db.Delete<UserModel>(u => u.ID == "AAAAAAAAAA");
        model = db.GetOrNull<UserModel>(u => u.ID == "AAAAAAAAAA");
        Ensure.Null(model);
        var ip = db.GetOrNull<IpAddressModel>(i => i.IP == "127.0.0.1");
        Ensure.Null(ip);
    }

    [Test]
    public void CacheDelete_UserModel_ReturnsExpected()
    {
        Logging.Enable();
        using var db = new DbService<UserSchema>(true);
        var model = db.Get<UserModel>(u => u.ID == "AAAAAAAAAA");
        Ensure.Equal("John", model.Name.Value());
        db.Delete<UserModel>(u => u.ID == "AAAAAAAAAA");
        var logs = Logging.GetHeldLogs("PostgreSQL: Query");
        Ensure.Count(logs, 0);

        Logging.Hold("PostgreSQL: Query");

        model = db.GetOrNull<UserModel>(u => u.ID == "AAAAAAAAAA");
        Ensure.Null(model);
        logs = Logging.GetHeldLogs("PostgreSQL: Query");
        Ensure.Count(logs, 2);
        Logging.ClearBuffer();
        // Cache Check
        Ensure.Count(Logging.GetHeldLogs("PostgreSQL: Query"), 0);
    }
}