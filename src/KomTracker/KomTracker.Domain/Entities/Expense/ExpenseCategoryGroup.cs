using System.Text.Json.Serialization;

namespace KomTracker.Domain.Entities.Expense;

/// <summary>UI group of an <see cref="ExpenseCategory"/> (code-side only, not persisted) — for the picker and filters.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExpenseCategoryGroup
{
    Clothing,
    Tyres,
    Parts,
    Workshop,
    Electronics,
    Accessories,
    Training,
    Nutrition,
    Other
}
