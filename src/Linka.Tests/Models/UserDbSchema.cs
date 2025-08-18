using Webamoki.Linka;
using Webamoki.Linka.Models;

namespace Linka.Tests.Models;

[method: Model<UserModel>]
[method: Model<IpAddressModel>]
internal class UserDbSchema() : DbSchema("u62560199300users", "User");