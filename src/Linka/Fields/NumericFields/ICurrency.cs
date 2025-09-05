namespace Webamoki.Linka.Fields.NumericFields;

public interface ICurrency
{
    // The currency symbol (e.g., "$", "€", "£")
    static abstract string Symbol { get; }

    // The ISO currency code (e.g., "USD", "EUR", "GBP")
    static abstract string IsoCode { get; }

    // Number of decimal places to display
    static abstract int DecimalPlaces { get; }

    // Whether symbol comes before (true) or after (false) the value
    static abstract bool SymbolPrefixed { get; }

    // Decimal separator character
    static abstract char DecimalSeparator { get; }

    // Thousands separator character
    static abstract char ThousandsSeparator { get; }
}

// Japanese Yen (¥)
public class Jpy : ICurrency
{
    public static string Symbol => "¥";

    public static string IsoCode => "JPY";

    public static int DecimalPlaces => 0; // No subunits

    public static bool SymbolPrefixed => true;

    public static char DecimalSeparator => '.';

    public static char ThousandsSeparator => ',';
}

// Euro (€)
public class Eur : ICurrency
{
    public static string Symbol => "€";

    public static string IsoCode => "EUR";

    public static int DecimalPlaces => 2;

    public static bool SymbolPrefixed => false; // Euro symbol after amount in many EU countries

    public static char DecimalSeparator => ',';

    public static char ThousandsSeparator => '.';
}

// British Pound (£)
public class Gbp : ICurrency
{
    public static string Symbol => "£";

    public static string IsoCode => "GBP";

    public static int DecimalPlaces => 2;

    public static bool SymbolPrefixed => true;

    public static char DecimalSeparator => '.';

    public static char ThousandsSeparator => ',';
}

// US Dollar ($)
public class Usd : ICurrency
{
    public static string Symbol => "$";

    public static string IsoCode => "USD";

    public static int DecimalPlaces => 2;

    public static bool SymbolPrefixed => true;

    public static char DecimalSeparator => '.';

    public static char ThousandsSeparator => ',';
}