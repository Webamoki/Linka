using Webamoki.Linka;
using Webamoki.Linka.Models;

namespace LinkaTests.Models;

[method: Model<UserModel>]
[method: Model<IpAddressModel>]
internal class UserDbSchema() : DbSchema("u62560199300users", "User");