using System.Globalization;

namespace Webamoki.Linka.Fields.NumericFields;

public class Price<T> where T : ICurrency
{
    public override int GetHashCode() =>
        MinorUnits;

    public readonly int MinorUnits;
    public Price(int minorUnits = 0) { MinorUnits = minorUnits; }

    protected Price(string value)
    {
        if (value.Contains('.'))
        {
            if (float.TryParse(value, out var f))
                MinorUnits = ConvertFloat(f);
        }

        if (int.TryParse(value, out var i))
            MinorUnits = i;

        MinorUnits = 0;
    }

    protected Price(float value) { MinorUnits = ConvertFloat(value); }

    protected Price(double value) { MinorUnits = ConvertFloat((float)value); }

    public string Display(bool includeSymbol = true)
    {
        var value = MajorUnits;

        var format = new NumberFormatInfo
        {
            NumberDecimalSeparator = T.DecimalSeparator.ToString(),
            NumberGroupSeparator = T.ThousandsSeparator.ToString(),
            NumberDecimalDigits = T.DecimalPlaces
        };

        var numberString = value.ToString("N", format);

        if (!includeSymbol)
            return numberString;

        return T.SymbolPrefixed
            ? $"{T.Symbol}{numberString}"
            : $"{numberString}{T.Symbol}";
    }

    public string IsoCode => T.IsoCode;

    private static int ConvertFloat(float input) => (int)Math.Round(input * MathF.Pow(10, T.DecimalPlaces));

    public float MajorUnits =>
        MinorUnits / MathF.Pow(10f, T.DecimalPlaces);

    public bool IsEmpty => MinorUnits == 0;

    public static Price<T> operator +(Price<T> price, int minorUnits) =>
        new(price.MinorUnits + minorUnits);

    public static Price<T> operator +(Price<T> left, Price<T> right) =>
        left + right.MinorUnits;

    public static Price<T> operator -(Price<T> price, int minorUnits) =>
        new(price.MinorUnits - minorUnits);

    public static Price<T> operator -(Price<T> left, Price<T> right) =>
        left - right.MinorUnits;

    public static Price<T> operator *(Price<T> price, double multiplier) =>
        new((int)Math.Round(price.MinorUnits * multiplier));

    public static Price<T> operator *(Price<T> left, Price<T> right) =>
        left * right.MinorUnits;

    public static Price<T> operator /(Price<T> price, double divisor) =>
        divisor == 0 ? new Price<T>() : new Price<T>((int)Math.Round(price.MinorUnits / divisor));

    public static Price<T> operator /(Price<T> left, Price<T> right) =>
        left / right.MinorUnits;

    public static bool operator ==(Price<T> left, Price<T> right) => left.MinorUnits == right.MinorUnits;
    public static bool operator !=(Price<T> left, Price<T> right) => left.MinorUnits != right.MinorUnits;
    public static bool operator <(Price<T> left, Price<T> right) => left.MinorUnits < right.MinorUnits;
    public static bool operator >(Price<T> left, Price<T> right) => left.MinorUnits > right.MinorUnits;
    public static bool operator <=(Price<T> left, Price<T> right) => left.MinorUnits <= right.MinorUnits;
    public static bool operator >=(Price<T> left, Price<T> right) => left.MinorUnits >= right.MinorUnits;

    private bool Equals(Price<T> other) => this == other;

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((Price<T>)obj);
    }
}

public class PriceDbField<T>(int max = 999999999) : IntDbField(0, max) where T : ICurrency
{
    public Price<T> Price => new(Value() ?? 0);
}