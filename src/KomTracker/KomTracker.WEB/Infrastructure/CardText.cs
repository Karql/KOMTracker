namespace KomTracker.WEB.Infrastructure;

/// <summary>Text helpers for list cards.</summary>
public static class CardText
{
    /// <summary>
    /// "Brand Model" subtitle; a non-breaking space when both are empty, so the line still takes up its height and
    /// cards with and without a brand/model stay aligned.
    /// </summary>
    public static string BrandModel(string? brand, string? model)
    {
        var text = $"{brand} {model}".Trim();
        return text.Length == 0 ? " " : text;
    }
}
