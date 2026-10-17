using KomTracker.Domain.Entities.Expense;
using KomTracker.WEB.Models.Component;

namespace KomTracker.WEB.Models.Expense;

/// <summary>
/// Hand-drawn line-art icons for expense categories, in the same style as <see cref="ComponentCategoryIcons"/>
/// (24×24 viewBox, stroke = currentColor). Where the item is the same as a component category (pump, lock, bottle,
/// lights…) the component symbol is reused; the rest are drawn here.
/// </summary>
public static class ExpenseCategoryIcons
{
    private static string G(string inner)
        => $"<g fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\" stroke-linejoin=\"round\">{inner}</g>";

    public static string Icon(ExpenseCategory category) => G(category switch
    {
        // Clothing
        ExpenseCategory.Clothing => ComponentCategoryIcons.ApparelSvg,
        ExpenseCategory.Shoes => ShoeSvg,
        ExpenseCategory.Cleats => CleatSvg,
        ExpenseCategory.Helmets => HelmetSvg,
        ExpenseCategory.Eyewear => GlassesSvg,

        // Tyres
        ExpenseCategory.Tubes => ComponentCategoryIcons.InnerTubeSvg,
        ExpenseCategory.Tubeless => ComponentCategoryIcons.DropSvg,

        // Parts
        ExpenseCategory.Cables => ComponentCategoryIcons.CableSvg,
        ExpenseCategory.Parts => ComponentCategoryIcons.BoltsSvg,

        // Workshop
        ExpenseCategory.Tools => ComponentCategoryIcons.ToolsetSvg,
        ExpenseCategory.Pumps => ComponentCategoryIcons.PumpSvg,
        ExpenseCategory.Lubricants => OilCanSvg,
        ExpenseCategory.Cleaning => SprayBottleSvg,

        // Electronics
        ExpenseCategory.Electronics => ComponentCategoryIcons.ComputerSvg,
        ExpenseCategory.Lights => ComponentCategoryIcons.LightsSvg,

        // Accessories
        ExpenseCategory.Bags => BagSvg,
        ExpenseCategory.Bottles => ComponentCategoryIcons.BottleSvg,
        ExpenseCategory.Locks => ComponentCategoryIcons.LockSvg,
        ExpenseCategory.Fenders => ComponentCategoryIcons.FendersSvg,
        ExpenseCategory.Accessories => ComponentCategoryIcons.TagSvg,

        // Training
        ExpenseCategory.Trainer => ComponentCategoryIcons.TrainerSvg,

        // Nutrition
        ExpenseCategory.Nutrition => GelSvg,

        _ => ComponentCategoryIcons.OtherSvg
    });

    // Cycling shoe in profile (sole + strap).
    private const string ShoeSvg = "<path d='M3 16c0-2.2 1.3-3.4 3.2-3.8L10 11.4l2.6-3.4 2.2.8-.8 3 4.6 1.8c1.4.6 2.4 1.6 2.4 2.8V17H3z'/><path d='M3 17.5h18.4'/><path d='M11 12.6l2.4.9'/>";

    // Road cleat (three-bolt plate).
    private const string CleatSvg = "<path d='M7 5h10l2.5 7L12 19l-7.5-7z'/><circle cx='12' cy='12' r='1.2'/><circle cx='9.4' cy='8.4' r='0.8'/><circle cx='14.6' cy='8.4' r='0.8'/>";

    // Vented road helmet.
    private const string HelmetSvg = "<path d='M3 15.5a9 8.5 0 0 1 18 0z'/><path d='M7.5 15.5v-3.2M12 15.5V9.2M16.5 15.5v-3.2'/><path d='M3 15.5h4.5l1.2 2'/>";

    // Sport glasses.
    private const string GlassesSvg = "<path d='M3 10.5h18'/><path d='M4 10.5c0 3.4 1.6 5 3.8 5s3.4-1.8 3.6-3.6M20 10.5c0 3.4-1.6 5-3.8 5s-3.4-1.8-3.6-3.6'/><path d='M11.4 11.9h1.2'/>";

    // Oil can with a spout.
    private const string OilCanSvg = "<rect x='4' y='11' width='10' height='8' rx='1.5'/><path d='M7 11V8.5h4V11'/><path d='M14 13l5.5-3.5'/><path d='M20 12.5v1.5'/>";

    // Trigger spray bottle.
    private const string SprayBottleSvg = "<path d='M8.5 10h6l1 2.2V20a1 1 0 0 1-1 1h-6a1 1 0 0 1-1-1v-7.8z'/><path d='M9.5 10V6.5h5l2.5 2'/><path d='M18.5 5.5l1.8-1M18.5 8.5h2'/>";

    // Energy gel sachet (torn-off tip) — sports nutrition.
    private const string GelSvg = "<path d='M8 21h8a1 1 0 0 0 1-1V8.5L14.5 3h-5L7 8.5V20a1 1 0 0 0 1 1z'/><path d='M9.5 3l1 2.2h3l1-2.2'/><path d='M7 11h10'/><path d='M12 14.2l-1.4 2.3h2.8L12 18.8'/>";

    // Saddle / frame bag.
    private const string BagSvg = "<rect x='4' y='8' width='16' height='10' rx='3'/><path d='M8 8V6.5a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2V8'/><path d='M4 12.5h16'/>";
}
