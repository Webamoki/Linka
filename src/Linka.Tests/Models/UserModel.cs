using Webamoki.Linka.Fields;
using Webamoki.Linka.Fields.NumericFields;
using Webamoki.Linka.Fields.TextFields;
using Webamoki.Linka.Models;

namespace Linka.Tests.Models;

internal class UserModel : Model
{
    [Key] [Unique] [FullText]
    public IdDbField ID { get; } = new();

    [FullText]
    public NameDbField Name { get; } = new();

    [FullText] [Unique]
    public EmailDbField Email { get; } = new();

    [FullText]
    public PhoneDbField Phone { get; } = new();

    public enum UserRank
    {
        User,
        Admin
    }

    public EnumDbField<UserRank> Rank { get; } = new();

    [Unique] [NotRequired]
    public HashDbField Session { get; } = new();

    public TextDbField Password { get; } = new();

    [NotRequired] // Navigation Field Property
    public IdDbField CartToken { get; } = new();

    public DateDbField Created { get; } = new();

    public BooleanDbField Verified { get; } = new();
    public BooleanDbField Login { get; } = new();
    public PriceDbField<Gbp> Credit { get; } = new();

    [PkNavigationList(nameof(IpAddressModel.UserID))]
    public List<IpAddressModel> IpAddresses = null!;
}