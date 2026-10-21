#nullable enable
using System.Collections.Generic;
using System.Linq;
using KomTracker.Domain.Entities.CostAnalysis;

namespace KomTracker.API.Shared.CostAnalysis;

/// <summary>
/// Colours of the cost page (D-COST-7). Sources have fixed colours; below them every group, category and item gets a
/// colour of its own from an 8-slot categorical palette, assigned once per report — so "Tire" is the same colour in the
/// donut, the year bars and the treemap, whatever the filter. Siblings never share a slot (until a parent has more
/// than 8 children); the largest child takes its parent's slot, so Wheels → Wheel stay alike; otherwise the slot
/// least used so far wins, so the biggest entities of the whole report all differ.
/// </summary>
public sealed class CostColors
{
    /// <summary>Categorical slots in their fixed (colour-vision-checked) order.</summary>
    public static readonly IReadOnlyList<string> Slots = new[]
    {
        "#2a78d6", // blue
        "#eb6834", // orange
        "#1baf7a", // aqua
        "#eda100", // yellow
        "#e87ba4", // magenta
        "#008300", // green
        "#6250d6", // violet
        "#e34948"  // red
    };

    /// <summary>The colour of everything past the 8th slice / series of a view, and of "Other".</summary>
    public const string Rest = "#9e9e9e";

    /// <summary>The top-level colours (chips, the sources donut, the year bars, the ledger).</summary>
    public static readonly IReadOnlyDictionary<CostSource, string> Sources = new Dictionary<CostSource, string>
    {
        [CostSource.Bikes] = "#1e88e5",
        [CostSource.Components] = "#fc4c02",
        [CostSource.Services] = "#8e24aa",
        [CostSource.Expenses] = "#00897b"
    };

    // The slot nearest each source's own colour — the source's largest group starts from it.
    private static readonly IReadOnlyDictionary<CostSource, int> SourceSlots = new Dictionary<CostSource, int>
    {
        [CostSource.Bikes] = 0,
        [CostSource.Components] = 1,
        [CostSource.Expenses] = 2,
        [CostSource.Services] = 6
    };

    private readonly Dictionary<string, int> _slots = new();

    private CostColors()
    {
    }

    /// <summary>Assigns every group / category / item of the report a slot, by its all-time spending.</summary>
    public static CostColors Build(CostAnalysisReport report)
    {
        var colors = new CostColors();
        var costs = report.Lines.Where(l => l.Kind == CostKind.Cost).ToList();

        colors.Assign(costs.GroupBy(l => GroupKey(l.Source, l.Group))
            .Select(g => (g.Key, Parent: g.First().Source.ToString(), Preferred: SourceSlots[g.First().Source], Amount: g.Sum(l => l.Amount))));

        colors.Assign(costs.GroupBy(l => CategoryKey(l.Source, l.Group, l.Category))
            .Select(g => (g.Key, Parent: GroupKey(g.First().Source, g.First().Group), Preferred: 0, Amount: g.Sum(l => l.Amount))));

        colors.Assign(costs.GroupBy(CostAggregator.ItemKey)
            .Select(g => (g.Key, Parent: CategoryKey(g.First().Source, g.First().Group, g.First().Category), Preferred: 0, Amount: g.Sum(l => l.Amount))));

        return colors;
    }

    // Largest first: the first child of a parent takes the parent's slot (a source's: its nearest slot); the next ones a
    // slot no sibling has yet, preferring the least used overall, then the palette order.
    private void Assign(IEnumerable<(string Key, string Parent, int Preferred, decimal Amount)> entities)
    {
        var usage = new int[Slots.Count];
        var siblings = new Dictionary<string, List<int>>();

        foreach (var (key, parent, preferred, _) in entities.OrderByDescending(e => e.Amount).ThenBy(e => e.Key))
        {
            if (!siblings.TryGetValue(parent, out var taken))
            {
                siblings[parent] = taken = new List<int>();
            }

            var start = _slots.TryGetValue(parent, out var parentSlot) ? parentSlot : preferred;
            var slot = taken.Count == 0
                ? start
                : Enumerable.Range(0, Slots.Count)
                    .Select(i => (start + i) % Slots.Count)
                    .OrderBy(s => taken.Count(t => t == s))
                    .ThenBy(s => usage[s])
                    .First();

            taken.Add(slot);
            usage[slot]++;
            _slots[key] = slot;
        }
    }

    public string Group(CostSource source, string group) => Of(GroupKey(source, group));

    public string Category(CostSource source, string group, string category) => Of(CategoryKey(source, group, category));

    /// <summary>The colour of a slice / series <paramref name="key"/> one level below <paramref name="drill"/>.</summary>
    public string Of(CostDrill drill, string key) => drill.Level switch
    {
        0 => System.Enum.TryParse<CostSource>(key, out var source) ? Sources[source] : Rest,
        1 => Group(drill.Source!.Value, key),
        2 => Category(drill.Source!.Value, drill.Group!, key),
        _ => Of(key)  // items: the key is CostAggregator.ItemKey
    };

    private string Of(string key) => _slots.TryGetValue(key, out var slot) ? Slots[slot] : Rest;

    private static string GroupKey(CostSource source, string group) => $"{source}|{group}";

    private static string CategoryKey(CostSource source, string group, string category) => $"{source}|{group}|{category}";
}
