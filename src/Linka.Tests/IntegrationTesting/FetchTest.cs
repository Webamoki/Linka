using NUnit.Framework;
using Tests.FixtureKit;
using Tests.FixtureKit.Fixtures;
using Webamoki.Linka;
using Webamoki.Linka.Testing;
using Webamoki.Utils.Testing;

namespace Tests.IntegrationTesting;

[Fixtures<UserModelFixture>]
[Fixtures<IpAddressFixture>]
public class FetchTest
{
    [Test]
    public void TestMethod()
    {
        using var db = new DbService<UserSchema>();
        var model = db.First<UserModel>(u=> u.ID == "AAAAAAAAAA");
        Ensure.Equal("John", model.Name.Value());
        Ensure.Equal("johndoe@example.com", model.Email.Value());
        Ensure.Equal("1234567890", model.Phone.Value());
        Ensure.Equal(UserModel.RankEnum.User, model.Rank.Value());
        Ensure.Equal("password", model.Password.Value());
        Ensure.Equal(null, model.CartToken.Value());
        Ensure.True(model.Verified.Value());
        Ensure.False(model.Login.Value());
        Ensure.Equal(100, model.Credit.Value());
    }
    
    [Test]
    public void TestMethod2()
    {
        var test = 1;
    }
}