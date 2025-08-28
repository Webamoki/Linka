using System.Linq.Expressions;
using NUnit.Framework;
using Tests.FixtureKit;
using Webamoki.Linka.Expressions;
using Webamoki.Linka.Testing;
using Webamoki.Utils;
using Webamoki.Utils.Testing;

namespace Tests.Expressions;

[CompileSchema<UserSchema>]
public class ExpressionBuilderTest
{

    private static string QuoteQuery(string query) =>
        query.Replace("`", "\"");

    [Test]
    public void GetQuery_RendersExpected()
    {
        var query = ExpressionBuilder.GetQuery<UserModel>();
        const string text = "SELECT `User`.`ID` as `User.ID` , `User`.`Name` as `User.Name` , `User`.`Email` as `User.Email` , `User`.`Phone` as `User.Phone` , `User`.`Rank` as `User.Rank` , `User`.`Session` as `User.Session` , `User`.`Password` as `User.Password` , `User`.`CartToken` as `User.CartToken` , `User`.`Created` as `User.Created` , `User`.`Verified` as `User.Verified` , `User`.`Login` as `User.Login` , `User`.`Credit` as `User.Credit` FROM `User`";
        
        Ensure.Equal(QuoteQuery(text),
            query.Render(out var values));
        Logging.WriteLog(query.Render(out  values));
        Ensure.Empty(values);
    }


    [Test]
    public void ConditionSimple_RightFormat_RendersExpected()
    {
        var valuesToCheck = new List<(Expression<Func<UserModel, bool>> expression, string sql, string? value)>
        {
            (a => a.ID == "AAAAAAAAAA", "`User`.`ID` = 'AAAAAAAAAA'", null),
            (a => a.ID != "AAAAAAAAAA", "`User`.`ID` != 'AAAAAAAAAA'", null),
            (a => a.Session == null, "`User`.`Session` IS NULL", null),
            (a => a.Session != null, "`User`.`Session` IS NOT NULL", null),
            (a => a.Rank == UserModel.RankEnum.User, "`User`.`Rank` = 'User'", null),
            (a => a.Verified == true, "`User`.`Verified` = true", null),
            (a => a.Login == false, "`User`.`Login` = false", null),
            (a => a.Created <= "2022-10-12", "`User`.`Created` <= '2022-10-12'", null),
            (a => a.Credit > 1023, "`User`.`Credit` > 1023", null),
            (a => a.Name == "Fred", "`User`.`Name` = ?", "Fred")
        };
        foreach (var (expression, expected, expectedValue) in valuesToCheck)
        {
            var query = ExpressionBuilder.Condition(
                expression,
                out var values,
                out var error
            );
            Ensure.Equal(QuoteQuery(expected), query);
            if (expectedValue != null)
            {
                Ensure.Count(values, 1);
                Ensure.Equal(expectedValue, values[0]);
            }
            else
            {
                Ensure.Empty(values);
            }

            Ensure.Null(error);
        }
    }

    [Test]
    public void ConditionSimple_Escaped_RendersExpected()
    {
        string? vNull = null;
        const bool vTrue = true;
        const bool vFalse = false;
        const string dateTime = "2022-10-12";
        const int vInt = 1023;
        const string vString = "Fred";
        const UserModel.RankEnum rank = UserModel.RankEnum.Admin;
        
        var valuesToCheck = new List<(Expression<Func<UserModel, bool>> expression, string sql, string? value)>
        {
            (a => a.Session == vNull, "`User`.`Session` IS NULL", null),
            (a => a.Session != vNull, "`User`.`Session` IS NOT NULL", null),
            (a => a.Rank == rank, "`User`.`Rank` = 'Admin'", null),
            (a => a.Verified == vTrue, "`User`.`Verified` = true", null),
            (a => a.Login == vFalse, "`User`.`Login` = false", null),
            (a => a.Created <= dateTime, "`User`.`Created` <= '2022-10-12'", null),
            (a => a.Credit > vInt, "`User`.`Credit` > 1023", null),
            (a => a.Name == vString, "`User`.`Name` = ?", "Fred")
        };
        foreach (var (expression, expected, expectedValue) in valuesToCheck)
        {
            var query = ExpressionBuilder.Condition(
                expression,
                out var values,
                out var error
            );
            Ensure.Equal(QuoteQuery(expected), query);
            if (expectedValue != null)
            {
                Ensure.Count(values, 1);
                Ensure.Equal(expectedValue, values[0]);
            }
            else
            {
                Ensure.Empty(values);
            }

            Ensure.Null(error);
        }
    }

    [Test]
    public void ConditionSimple_WrongFormat_ReturnsErrors()
    {
        var valuesToCheck = new List<(Expression<Func<UserModel, bool>> expression, string error)>
        {
            (a => a.ID == "F", "Invalid value for field ID: Value length is not 10"),
            (a => a.ID == null, "Invalid value for field ID: Value cannot be null")
        };
        foreach (var (expression, expectedError) in valuesToCheck)
        {
            var query = ExpressionBuilder.Condition(
                expression,
                out var values,
                out var error
            );
            Ensure.Empty(query);
            Ensure.Empty(values);
            Ensure.Equal(expectedError, error);
        }
    }
    
    
    [Test]
    public void Condition_Layered_RendersExpected()
    {
        var valuesToCheck = new List<(Expression<Func<UserModel, bool>> expression, string sql, List<string>? value)>
        {
            // 2 statements, 1 layer
            (a => a.ID == "AAAAAAAAAA" || a.ID == "BBBBBBBBBB", "`User`.`ID` = 'AAAAAAAAAA' OR `User`.`ID` = 'BBBBBBBBBB'", null),
            (a => a.ID == "AAAAAAAAAA" || a.ID == "BBBBBBBBBB", "`User`.`ID` = 'AAAAAAAAAA' OR `User`.`ID` = 'BBBBBBBBBB'", null),
            (a => a.ID != "AAAAAAAAAA" && a.ID != "BBBBBBBBBB", "`User`.`ID` != 'AAAAAAAAAA' AND `User`.`ID` != 'BBBBBBBBBB'", null),
            (a => a.Session == null || a.Session != "sfffaaaaa", "`User`.`Session` IS NULL OR `User`.`Session` != 'sfffaaaaa'", null),
            (a => a.Name == "Fred" || a.Name == "George", "`User`.`Name` = ? OR `User`.`Name` = ?",
                ["Fred", "George"]),
            // 3-4 statements, 2 layers
            (a => a.ID == "AAAAAAAAAA" || a.ID == "BBBBBBBBBB" && a.ID != "CCCCCCCCCC", "`User`.`ID` = 'AAAAAAAAAA' OR (`User`.`ID` = 'BBBBBBBBBB' AND `User`.`ID` != 'CCCCCCCCCC')", null),
            (a => a.ID != "AAAAAAAAAA" && a.ID != "BBBBBBBBBB" || a.ID == "CCCCCCCCCC", "(`User`.`ID` != 'AAAAAAAAAA' AND `User`.`ID` != 'BBBBBBBBBB') OR `User`.`ID` = 'CCCCCCCCCC'", null),
            (a => a.ID != "AAAAAAAAAA" && (a.ID != "BBBBBBBBBB" || a.ID == "CCCCCCCCCC"), "`User`.`ID` != 'AAAAAAAAAA' AND (`User`.`ID` != 'BBBBBBBBBB' OR `User`.`ID` = 'CCCCCCCCCC')", null),
            (a => a.Session == null || (a.Session != "sfffaaaaa" && a.Session == "sfffaaaaa"), "`User`.`Session` IS NULL OR (`User`.`Session` != 'sfffaaaaa' AND `User`.`Session` = 'sfffaaaaa')", null),
            (a => a.Name == "Fred" || (a.Name == "George" && a.Name == "John"), "`User`.`Name` = ? OR (`User`.`Name` = ? AND `User`.`Name` = ?)", ["Fred", "George", "John"])
        };
        foreach (var (expression, expected, expectedValues) in valuesToCheck)
        {
            var query = ExpressionBuilder.Condition(
                expression,
                out var values,
                out var error
            );
            Ensure.Equal(QuoteQuery(expected), query);
            if (expectedValues != null)
            {
                Ensure.Count(values, expectedValues.Count);
                for (var i = 0; i < expectedValues.Count; i++)
                {
                    Ensure.Equal(expectedValues[i], values[i]);
                }
            }
            else
            {
                Ensure.Empty(values);
            }

            Ensure.Null(error);
        }
    }
}