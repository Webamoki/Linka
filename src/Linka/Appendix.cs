namespace Webamoki.Linka;

internal static class Appendix
{
    public static string Constraint(string tableName, string navigationName) =>
        $"CONSTRAINT_{tableName}_{navigationName}";
    
    public static string ConstraintPrefix(string tableName) => $"CONSTRAINT_{tableName}_";
    public static string UniqueKey(string fieldName) =>
        $"U_{fieldName}";
}