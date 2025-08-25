using Webamoki.Linka;
using Webamoki.Linka.Models;

namespace Tests.Models;

[method: Model<UserModel>]
[method: Model<IpAddressModel>]
[method: Enum<IpAddressModel>]
public class UserDbSchema() : DbSchema("users", "User");