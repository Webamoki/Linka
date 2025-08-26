using Webamoki.Linka;
using Webamoki.Linka.ModelSystem;
using Webamoki.Linka.SchemaSystem;

namespace Tests.FixtureKit;

[method: Enum<UserModel.RankEnum>]
[method: Model<UserModel>]
[method: Model<IpAddressModel>]
public class UserSchema() : Schema("User");