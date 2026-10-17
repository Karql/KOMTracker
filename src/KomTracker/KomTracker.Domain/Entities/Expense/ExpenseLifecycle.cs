using System.Text.Json.Serialization;

namespace KomTracker.Domain.Entities.Expense;

/// <summary>
/// Lifecycle state of an expense (still owned / used up or thrown away / resold). Persisted and serialized by name.
/// Active expenses show by default; Archived/Sold are hidden unless requested (like components).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExpenseLifecycle
{
    Active,
    Archived,
    Sold
}
