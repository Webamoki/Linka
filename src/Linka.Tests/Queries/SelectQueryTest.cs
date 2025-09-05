using NUnit.Framework;
using Webamoki.Linka.Queries;
using Webamoki.Utils.Testing;

namespace Tests.Queries;

public class SelectQueryTest
{
    private static string QuoteQuery(string query) =>
        query.Replace("`", "\"");

    [Test]
    public void Render_EmptyQuery_ThrowsException()
    {
        var query = new SelectQuery();
        Ensure.Throws(() => query.Render(out _), "No tables specified");
        query.AddBody("SELECT");
        Ensure.Throws(() => query.Render(out _), "No tables specified");
    }

    [Test]
    public void Render_Table_RendersExpected()
    {
        var query = new SelectQuery();
        query.AddTable("Test");
        var rQuery = query.Render(out var rValues);
        Ensure.Equal(QuoteQuery("SELECT * FROM `Test`"), rQuery);
        Ensure.Empty(rValues);
    }

    [Test]
    public void Render_TableAlias_RendersExpected()
    {
        var query = new SelectQuery();
        query.AddTable("Test", "TestAlias");
        var rQuery = query.Render(out var rValues);
        Ensure.Equal(QuoteQuery("SELECT * FROM `Test` `TestAlias`"), rQuery);
        Ensure.Empty(rValues);
    }

    [Test]
    public void Render_MultipleTables_RendersExpected()
    {
        var query = new SelectQuery();
        query.AddTable("Test");
        query.AddTable("Test1");
        query.AddTable("Test2");
        query.AddTable("Test3");
        var rQuery = query.Render(out var rValues);
        Ensure.Equal(QuoteQuery("SELECT * FROM `Test` , `Test1` , `Test2` , `Test3`"), rQuery);
        Ensure.Empty(rValues);
    }

    [Test]
    public void Render_WithModification_RendersExpected()
    {
        var query = new SelectQuery();
        query.AddTable("Test");
        var rQuery = query.Render(out var rValues);
        Ensure.Equal(QuoteQuery("SELECT * FROM `Test`"), rQuery);
        Ensure.Empty(rValues);
        query.AddTable("Test1");
        var rQuery2 = query.Render(out var rValues2);
        Ensure.Equal(QuoteQuery("SELECT * FROM `Test` , `Test1`"), rQuery2);
        Ensure.Empty(rValues2);
    }

    [Test]
    public void Render_OffsetLimit_RendersExpected()
    {
        var query = new SelectQuery();
        query.AddTable("Test");
        query.Offset = 10;
        var rQuery = query.Render(out var rValues);
        Ensure.Equal(QuoteQuery("SELECT * FROM `Test` OFFSET 10"), rQuery);
        Ensure.Empty(rValues);
        query.Limit = 20;
        var rQuery2 = query.Render(out var rValues2);
        Ensure.Equal(QuoteQuery("SELECT * FROM `Test` LIMIT 20 OFFSET 10"), rQuery2);
        Ensure.Empty(rValues2);
        query.Offset = 0;
        var rQuery3 = query.Render(out var rValues3);
        Ensure.Equal(QuoteQuery("SELECT * FROM `Test` LIMIT 20"), rQuery3);
        Ensure.Empty(rValues3);
    }
}