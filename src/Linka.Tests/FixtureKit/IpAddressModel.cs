using Webamoki.Linka.Fields;
using Webamoki.Linka.Fields.TextFields;
using Webamoki.Linka.Models;

namespace Tests.FixtureKit;

public class IpAddressModel(): Model
{
    [Key]
    public IdDbField UserID { get; } = new();

    [Key] [FullText]
    public TextDbField IP { get; } = new();

    public DateTimeDbField Accessed { get; } = new();

    [PkNavigation(nameof(UserID),NavConstraint.Cascade)]
    public UserModel User = null!;
    
    
    public IpAddressModel(string userID, string ip): this()
    {
        UserID.Value(userID);
        IP.Value(ip);
        Accessed.SetNow();
    }
}