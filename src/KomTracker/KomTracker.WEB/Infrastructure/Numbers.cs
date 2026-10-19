using System.Globalization;

namespace KomTracker.WEB.Infrastructure;

/// <summary>
/// The app's number display: thousands grouped with a no-break space ("12 418" — never wraps inside a table cell),
/// "." as the decimal separator. Mileage figures use <see cref="Mileage"/> so every table/panel reads the same:
/// distance and elevation as whole numbers, moving time in whole hours.
/// </summary>
public static class Numbers
{
    private static readonly NumberFormatInfo Nfi = new()
    {
        NumberGroupSeparator = " ",
        NumberDecimalSeparator = "."
    };

    /// <summary>Grouped number with a fixed number of decimals (default none, rounded).</summary>
    public static string Format(decimal value, int decimals = 0) => value.ToString("N" + decimals, Nfi);

    /// <inheritdoc cref="Format(decimal, int)"/>
    public static string Format(double value, int decimals = 0) => value.ToString("N" + decimals, Nfi);

    /// <summary>Grouped number with up to two decimals, only when needed ("1 500", "19 723.05") — for money.</summary>
    public static string Amount(decimal value) => value.ToString("#,0.##", Nfi);

    public static string Km(decimal km) => $"{Format(km)} km";
    public static string Hours(decimal hours) => $"{Format(hours)} h";
    public static string Meters(decimal meters) => $"{Format(meters)} m";

    /// <summary>
    /// "distance · moving time · elevation" (e.g. "12 418 km · 486 h · 98 200 m"), leaving out the parts that round to
    /// zero; "—" when nothing is left.
    /// </summary>
    public static string Mileage(decimal? km, decimal? hours, decimal? meters)
    {
        var parts = new List<string>();
        if (km is { } d && Math.Round(d) > 0) parts.Add(Km(d));
        if (hours is { } h && Math.Round(h) > 0) parts.Add(Hours(h));
        if (meters is { } m && Math.Round(m) > 0) parts.Add(Meters(m));
        return parts.Count == 0 ? "—" : string.Join(" · ", parts);
    }
}
