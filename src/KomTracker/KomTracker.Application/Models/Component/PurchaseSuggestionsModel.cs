namespace KomTracker.Application.Models.Component;

/// <summary>Distinct past values (from the user's bikes + components) offered as autocomplete hints.</summary>
public record PurchaseSuggestionsModel(
    IReadOnlyList<string> Brands,
    IReadOnlyList<string> Models,
    IReadOnlyList<string> PurchasePlaces);
