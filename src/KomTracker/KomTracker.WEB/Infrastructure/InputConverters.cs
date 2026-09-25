using System.Globalization;

namespace KomTracker.WEB.Infrastructure;

/// <summary>Shared MudBlazor input converters.</summary>
public static class InputConverters
{
    /// <summary>
    /// Decimal converter that accepts BOTH ',' and '.' as the decimal separator (so "12,50" and "12.50" both parse),
    /// and displays with InvariantCulture (dot). Apply to a `MudNumericField` via `Converter=`.
    /// </summary>
    /// <summary>For nullable decimal fields (blank ⇒ null).</summary>
    public static MudBlazor.DeferredConverter<decimal?, string> Decimal { get; } = BuildDecimal();

    /// <summary>For non-nullable decimal fields (blank/invalid ⇒ 0).</summary>
    public static MudBlazor.DeferredConverter<decimal, string> DecimalRequired { get; } = BuildDecimalRequired();

    private static MudBlazor.DeferredConverter<decimal?, string> BuildDecimal()
    {
        var converter = new MudBlazor.DeferredConverter<decimal?, string>();

        converter.Set(
            value => value?.ToString(CultureInfo.InvariantCulture),
            text => TryParse(text, out var value) ? value : (decimal?)null);

        return converter;
    }

    private static MudBlazor.DeferredConverter<decimal, string> BuildDecimalRequired()
    {
        var converter = new MudBlazor.DeferredConverter<decimal, string>();

        converter.Set(
            value => value.ToString(CultureInfo.InvariantCulture),
            text => TryParse(text, out var value) ? value : 0m);

        return converter;
    }

    // Accepts both ',' and '.' as the decimal separator; parses with InvariantCulture.
    private static bool TryParse(string? text, out decimal value)
    {
        value = 0m;

        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var normalized = text.Trim().Replace(',', '.');
        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
    }
}
