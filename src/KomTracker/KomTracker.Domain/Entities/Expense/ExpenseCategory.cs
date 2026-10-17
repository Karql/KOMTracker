using System.Text.Json.Serialization;

namespace KomTracker.Domain.Entities.Expense;

/// <summary>
/// Expense category (D-EXP-1) — single-word buckets modelled on big bike shops' menus. Persisted and serialized by
/// name (string): renaming a member requires a data migration. UI grouping + ordering come from
/// <see cref="ExpenseCategoryMetadata"/>. <see cref="Other"/> is the fallback.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExpenseCategory
{
    // Clothing
    Clothing,
    Shoes,
    Cleats,
    Helmets,
    Eyewear,

    // Tyres
    Tubes,
    Tubeless,

    // Parts
    Cables,
    Parts,

    // Workshop
    Tools,
    Pumps,
    Lubricants,
    Cleaning,

    // Electronics
    Electronics,
    Lights,

    // Accessories
    Bags,
    Bottles,
    Locks,
    Fenders,
    Accessories,

    // Training
    Trainer,

    // Nutrition
    Nutrition,

    Other
}
