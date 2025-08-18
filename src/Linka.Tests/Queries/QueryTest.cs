using NUnit.Framework;
using Webamoki.Linka.Queries;
using Webamoki.TestUtils;

namespace Linka.Tests.Queries;

public class QueryTest
{
    [Test]
    public void Render_Query()
    {
        var query = new Query();
        query.AddBody("query1", "query2");
        var rQuery = query.Render(out var rValues);
        Ensure.Equal("query1 query2", rQuery);
        Ensure.Empty(rValues);
    }

    [Test]
    public void Render_ValueQuery_ThrowsException()
    {
        var query = new Query();
        query.AddBody("query1 = ?", "query2 = ?");
        Ensure.Throws(() => query.Render(out _), "Not enough values to execute query.");
    }

    [Test]
    public void Render_ValueQuery()
    {
        var query = new Query();
        query.AddBody("query1 = ?", "query2 = ?", "query3 = ? ?");
        query.AddValue("value0");
        query.AddValue("value1");
        query.AddValue("value2");
        query.AddValue("value3");
        var rQuery = query.Render(out var rValues);
        Ensure.Equal("query1 = ? query2 = ? query3 = ? ?", rQuery);
        Ensure.Count(rValues, 4);
        Ensure.Equal("value0", rValues[0]);
        Ensure.Equal("value1", rValues[1]);
        Ensure.Equal("value2", rValues[2]);
        Ensure.Equal("value3", rValues[3]);
    }

    [Test]
    public void Render_NestedQuery()
    {
        var query1 = new Query();
        query1.AddBody("query0 = ?");
        query1.AddValue("value0");
        var query2 = new Query();
        query2.AddBody("query1 = ?", query1);
        query2.AddValue("value1");
        var rQuery = query2.Render(out var rValues);
        Ensure.Equal("query1 = ? query0 = ?", rQuery);
        Ensure.Count(rValues, 2);
        Ensure.Equal("value1", rValues[0]);
        Ensure.Equal("value0", rValues[1]);
    }
}