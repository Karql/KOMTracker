using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KomTracker.Domain.Entities.Component;

/// <summary>
/// Code-side metadata for <see cref="ComponentCategory"/> (D-13) — currently just the UI grouping used to render
/// a grouped category picker. No lookup table; no installable/eligibility flag (decoupled from category).
/// </summary>
public static class ComponentCategoryMetadata
{
    /// <summary>The <see cref="ComponentCategoryGroup"/> a category belongs to. <see cref="ComponentCategory.Other"/> → Accessories.</summary>
    public static ComponentCategoryGroup Group(ComponentCategory category) => category switch
    {
        ComponentCategory.Brake or ComponentCategory.BrakeCaliper or ComponentCategory.BrakeLever
            or ComponentCategory.BrakePads or ComponentCategory.BrakeRotor => ComponentCategoryGroup.Brakes,

        ComponentCategory.Chain or ComponentCategory.Cassette or ComponentCategory.Chainring
            or ComponentCategory.Crankset or ComponentCategory.FrontDerailleur or ComponentCategory.RearDerailleur
            or ComponentCategory.Shifter or ComponentCategory.Pulley or ComponentCategory.Chainguide
            or ComponentCategory.Sprocket => ComponentCategoryGroup.Drivetrain,

        ComponentCategory.Handlebar or ComponentCategory.BarTape or ComponentCategory.Grips
            or ComponentCategory.Stem => ComponentCategoryGroup.Cockpit,

        ComponentCategory.Wheel or ComponentCategory.Tire or ComponentCategory.TireInsert or ComponentCategory.Hub
            or ComponentCategory.Spokes or ComponentCategory.RimTape or ComponentCategory.InnerTube
            or ComponentCategory.TubelessSealant or ComponentCategory.ThruAxle => ComponentCategoryGroup.Wheels,

        ComponentCategory.Frame or ComponentCategory.Fork or ComponentCategory.Headset
            or ComponentCategory.BottomBracket or ComponentCategory.Bearing or ComponentCategory.Bolts
            or ComponentCategory.Pedals or ComponentCategory.Saddle or ComponentCategory.Seatpost => ComponentCategoryGroup.Structure,

        ComponentCategory.SuspensionFork or ComponentCategory.RearShock or ComponentCategory.DropperSeatpost
            or ComponentCategory.SuspensionSeatpost => ComponentCategoryGroup.Suspension,

        ComponentCategory.Cable or ComponentCategory.HydraulicLines => ComponentCategoryGroup.Cables,

        ComponentCategory.Battery or ComponentCategory.Motor => ComponentCategoryGroup.Electric,

        ComponentCategory.Trainer or ComponentCategory.IndoorBike or ComponentCategory.Fan
            or ComponentCategory.Mat or ComponentCategory.Riser => ComponentCategoryGroup.Indoor,

        _ => ComponentCategoryGroup.Accessories
    };

    /// <summary>
    /// Categories grouped for the picker: groups in <see cref="ComponentCategoryGroup"/> order, categories in
    /// <see cref="ComponentCategory"/> declaration order within each group.
    /// </summary>
    public static IReadOnlyList<(ComponentCategoryGroup Group, IReadOnlyList<ComponentCategory> Categories)> CategoriesByGroup()
        => Enum.GetValues<ComponentCategory>()
            .GroupBy(Group)
            .OrderBy(g => g.Key)
            .Select(g => (g.Key, (IReadOnlyList<ComponentCategory>)g.OrderBy(DisplayName, StringComparer.OrdinalIgnoreCase).ToList()))
            .ToList();

    /// <summary>Human-friendly category label (e.g. "Bar Tape" instead of "BarTape").</summary>
    public static string DisplayName(ComponentCategory category) => category switch
    {
        ComponentCategory.BellHorn => "Bell / Horn",
        _ => Humanize(category.ToString())
    };

    /// <summary>Human-friendly group label.</summary>
    public static string DisplayName(ComponentCategoryGroup group) => group.ToString();

    // Split a PascalCase identifier into spaced words ("TubelessSealant" → "Tubeless Sealant").
    private static string Humanize(string pascal)
    {
        var sb = new StringBuilder(pascal.Length + 4);
        for (var i = 0; i < pascal.Length; i++)
        {
            if (i > 0 && char.IsUpper(pascal[i]))
            {
                sb.Append(' ');
            }

            sb.Append(pascal[i]);
        }

        return sb.ToString();
    }
}
