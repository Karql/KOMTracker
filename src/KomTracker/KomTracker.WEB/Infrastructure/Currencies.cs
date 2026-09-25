using System.Globalization;

namespace KomTracker.WEB.Infrastructure;

/// <summary>A supported display currency: ISO code, symbol, name, and whether the symbol goes after the amount.</summary>
public record CurrencyInfo(string Code, string Symbol, string Name, bool SymbolAfter);

/// <summary>
/// The supported currency set (single source for the account combo + amount formatting). Add new entries here;
/// each carries its symbol placement (e.g. PLN's "zł" trails the amount, others lead).
/// </summary>
public static class Currencies
{
    public const string Default = "PLN";

    public static readonly IReadOnlyList<CurrencyInfo> All = new[]
    {
        new CurrencyInfo("CHF", "CHF", "Swiss Franc", SymbolAfter: false),
        new CurrencyInfo("EUR", "€", "Euro", SymbolAfter: false),
        new CurrencyInfo("GBP", "£", "British Pound", SymbolAfter: false),
        new CurrencyInfo("USD", "$", "US Dollar", SymbolAfter: false),
        new CurrencyInfo("PLN", "zł", "Polish Zloty", SymbolAfter: true),
    };

    public static CurrencyInfo Get(string? code)
        => All.FirstOrDefault(c => c.Code == code) ?? All.First(c => c.Code == Default);

    /// <summary>Combo display, e.g. "zł PLN - Polish Zloty".</summary>
    public static string Display(CurrencyInfo c) => $"{c.Symbol} {c.Code} - {c.Name}";

    /// <summary>Format an amount with just the currency symbol, placed per the currency (e.g. "1500 zł" / "$1500").</summary>
    public static string Format(decimal amount, string? code)
    {
        var c = Get(code);
        var number = amount.ToString("0.##", CultureInfo.InvariantCulture);
        return c.SymbolAfter ? $"{number} {c.Symbol}" : $"{c.Symbol}{number}";
    }
}
