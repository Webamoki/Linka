namespace Webamoki.Linka.Queries;

internal class InsertQuery(string table) : Query
{
    private readonly List<string> _columns = [];
    private readonly List<string> _valueMarkers = [];
    private int _values;
    public void AddColumn(string column) => _columns.Add(column);

    public override void AddValues(List<object> values)
    {
        _values++;
        base.AddValues(values);
    }

    public void AddValueMarker()
    {
        AddValueMarker("?");
    }
    public void AddValueMarker(object marker)
    {
        _valueMarkers.Add((String)marker);
    }
    internal override string Render(out List<object> values)
    {
        ResetBody();

        if (_columns.Count == 0) throw new Exception("No columns have been specified for the INSERT query.");

        if (_values == 0)
            throw new Exception("No values have been provided for the INSERT query. Call AddValues() to add a row.");

        var formattedColumns = $"\"{string.Join("\",\"", _columns)}\"";

        var singleRowPlaceholder = $"({string.Join(",", _valueMarkers)})";

        // e.g., (?,?),(?,?) for two rows with two columns each.
        var allValuePlaceholders = string.Join(",", Enumerable.Repeat(singleRowPlaceholder, _values));

        AddBody($"INSERT INTO \"{table}\" ({formattedColumns}) VALUES {allValuePlaceholders};");

        return base.Render(out values);
    }
}