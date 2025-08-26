using Webamoki.Linka;
using Webamoki.Linka.Models;

namespace Tests.FixtureKit;

[method: Enum<UserModel.RankEnum>]
[method: Model<UserModel>]
[method: Model<IpAddressModel>]
public class UserDbSchema() : DbSchema("User");