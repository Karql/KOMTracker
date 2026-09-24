using System;
using System.Collections.Generic;

namespace KomTracker.API.Shared.ViewModels.Suggestions;

/// <summary>Autocomplete hints for the add/edit dialogs — distinct past values from the user's bikes + components.</summary>
public class PurchaseSuggestionsViewModel
{
    public IReadOnlyCollection<string> Brands { get; set; } = Array.Empty<string>();
    public IReadOnlyCollection<string> Models { get; set; } = Array.Empty<string>();
    public IReadOnlyCollection<string> PurchasePlaces { get; set; } = Array.Empty<string>();
}
