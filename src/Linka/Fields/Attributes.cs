namespace Webamoki.Linka.Fields;

/// <summary>
///     Defines a primary key field in a model.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class KeyAttribute : Attribute;

/// <summary>
///     Defines a unique field in a model.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class UniqueAttribute : Attribute;

/// <summary>
///     Defines a full-text search field in a model.
/// </summary>
/// <param name="weight"></param>
[AttributeUsage(AttributeTargets.Property)]
public class FullTextAttribute(int weight) : Attribute
{
    public FullTextAttribute() : this(1) { }

    public int Weight { get; } = weight;
}

/// <summary>
///     Defines a field that is not required (Nullable) in a model.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class NotRequiredAttribute : Attribute;

/// <summary>
///     Defines the abstract base class for navigation attributes.
/// </summary>
/// <param name="field"></param>
/// <param name="targetField"></param>
public abstract class BaseNavigationAttribute(string field, string targetField)
    : Attribute
{
    public string Field { get; } = field;

    public string TargetField { get; } = targetField;
}

[AttributeUsage(AttributeTargets.Field)]
public class NavigationListAttribute(string field, string targetField)
    : BaseNavigationAttribute(field, targetField);

[AttributeUsage(AttributeTargets.Field)]
public class PkNavigationListAttribute(string targetField)
    : NavigationListAttribute("", targetField);

public enum NavConstraint
{
    Cascade,
    SetNull,
    Restrict,
    NoAction
}

public static class NavConstraintExtensions
{
    public static string ToSqlString(this NavConstraint constraint)
    {
        return constraint switch
        {
            NavConstraint.Cascade => "CASCADE",
            NavConstraint.SetNull => "SET NULL",
            NavConstraint.Restrict => "RESTRICT",
            NavConstraint.NoAction => "NO ACTION",
            _ => throw new ArgumentOutOfRangeException(nameof(constraint), constraint, null)
        };
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class NavigationAttribute(string field, string targetField, NavConstraint constraint = NavConstraint.Restrict)
    : BaseNavigationAttribute(field, targetField)
{
    public NavConstraint Constraint { get; } = constraint;
}

[AttributeUsage(AttributeTargets.Field)]
public class PkNavigationAttribute(string field, NavConstraint constraint = NavConstraint.Restrict)
    : NavigationAttribute(field, "", constraint);