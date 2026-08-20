using KomTracker.Domain.Entities.Component;

namespace KomTracker.WEB.Models.Component;

/// <summary>
/// Hand-drawn, monochromatic line-art icons for component categories (a WEB/presentation concern — kept out of
/// Domain). Each icon is inner SVG markup (24×24 viewBox, stroke = currentColor) that MudIcon wraps in an svg,
/// so it inherits the theme colour like the built-in icons. Every category has a distinct symbol; the per-group
/// icon is only a last-resort fallback.
/// <para>All symbols are <c>const</c> fields; <see cref="Icon"/>/<see cref="GroupSvg"/> only map to them.</para>
/// </summary>
public static class ComponentCategoryIcons
{
    private static string G(string inner)
        => $"<g fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\" stroke-linejoin=\"round\">{inner}</g>";

    public static string Icon(ComponentCategory category) => G(category switch
    {
        // Brakes
        ComponentCategory.Brake => BrakeSvg,
        ComponentCategory.BrakeCaliper => BrakeCaliperSvg,
        ComponentCategory.BrakeLever => BrakeLeverSvg,
        ComponentCategory.BrakePads => BrakePadsSvg,
        ComponentCategory.BrakeRotor => RotorSvg,

        // Drivetrain
        ComponentCategory.Chain => ChainSvg,
        ComponentCategory.Cassette => CassetteSvg,
        ComponentCategory.Chainring => ChainringSvg,
        ComponentCategory.Crankset => CranksetSvg,
        ComponentCategory.FrontDerailleur => FrontDerailleurSvg,
        ComponentCategory.RearDerailleur => RearDerailleurSvg,
        ComponentCategory.Shifter => ShifterSvg,
        ComponentCategory.Pulley => PulleySvg,
        ComponentCategory.Chainguide => ChainguideSvg,
        ComponentCategory.Sprocket => SprocketSvg,

        // Cockpit
        ComponentCategory.Handlebar => HandlebarSvg,
        ComponentCategory.BarTape => BarTapeSvg,
        ComponentCategory.Grips => GripsSvg,
        ComponentCategory.Stem => StemSvg,

        // Wheels
        ComponentCategory.Wheel => WheelSvg,
        ComponentCategory.Tire => TireSvg,
        ComponentCategory.TireInsert => TireInsertSvg,
        ComponentCategory.Hub => HubSvg,
        ComponentCategory.Spokes => SpokesSvg,
        ComponentCategory.RimTape => RimTapeSvg,
        ComponentCategory.InnerTube => InnerTubeSvg,
        ComponentCategory.TubelessSealant => DropSvg,
        ComponentCategory.ThruAxle => ThruAxleSvg,

        // Structure
        ComponentCategory.Frame => FrameSvg,
        ComponentCategory.Fork => ForkSvg,
        ComponentCategory.Headset => HeadsetSvg,
        ComponentCategory.BottomBracket => BottomBracketSvg,
        ComponentCategory.Bearing => BearingSvg,
        ComponentCategory.Bolts => BoltsSvg,
        ComponentCategory.Pedals => PedalsSvg,
        ComponentCategory.Saddle => SaddleSvg,
        ComponentCategory.Seatpost => SeatpostSvg,

        // Suspension
        ComponentCategory.SuspensionFork => SuspensionForkSvg,
        ComponentCategory.RearShock => ShockSvg,
        ComponentCategory.DropperSeatpost => DropperSeatpostSvg,
        ComponentCategory.SuspensionSeatpost => SuspensionSeatpostSvg,

        // Cables
        ComponentCategory.Cable => CableSvg,
        ComponentCategory.HydraulicLines => HydraulicLinesSvg,

        // Electric
        ComponentCategory.Battery => BatterySvg,
        ComponentCategory.Motor => MotorSvg,

        // Indoor
        ComponentCategory.Trainer => TrainerSvg,
        ComponentCategory.IndoorBike => IndoorBikeSvg,
        ComponentCategory.Fan => FanSvg,
        ComponentCategory.Mat => MatSvg,
        ComponentCategory.Riser => RiserSvg,

        // Accessories
        ComponentCategory.Computer => ComputerSvg,
        ComponentCategory.Lights => LightsSvg,
        ComponentCategory.Lock => LockSvg,
        ComponentCategory.Pump => PumpSvg,
        ComponentCategory.Rack => RackSvg,
        ComponentCategory.Bottle => BottleSvg,
        ComponentCategory.BellHorn => BellHornSvg,
        ComponentCategory.Fenders => FendersSvg,
        ComponentCategory.Kickstand => KickstandSvg,
        ComponentCategory.Toolset => ToolsetSvg,
        ComponentCategory.Apparel => ApparelSvg,
        ComponentCategory.Accessories => TagSvg,
        ComponentCategory.Other => OtherSvg,

        _ => GroupSvg(ComponentCategoryMetadata.Group(category))
    });

    public static string GroupIcon(ComponentCategoryGroup group) => G(GroupSvg(group));

    private static string GroupSvg(ComponentCategoryGroup group) => group switch
    {
        ComponentCategoryGroup.Brakes => RotorSvg,
        ComponentCategoryGroup.Drivetrain => ChainringSvg,
        ComponentCategoryGroup.Cockpit => HandlebarSvg,
        ComponentCategoryGroup.Wheels => WheelSvg,
        ComponentCategoryGroup.Structure => FrameSvg,
        ComponentCategoryGroup.Suspension => ShockSvg,
        ComponentCategoryGroup.Cables => CableSvg,
        ComponentCategoryGroup.Electric => BatterySvg,
        ComponentCategoryGroup.Indoor => TrainerSvg,
        _ => TagSvg
    };

    // ── Brakes ──
    private const string BrakeSvg = "<circle cx='10' cy='12' r='7.5'/><circle cx='10' cy='12' r='2.5'/><rect x='15.5' y='9' width='4.5' height='6' rx='1'/>";
    private const string BrakeCaliperSvg = "<path d='M12 3v18'/><path d='M9 8h5a2 2 0 0 1 2 2v4a2 2 0 0 1-2 2H9'/><path d='M12 11h4M12 13h4'/>";
    private const string BrakeLeverSvg = "<circle cx='6' cy='6' r='1.3'/><path d='M4 6h2.6'/><path d='M6.8 7C8 11 11 14.5 17 16'/><circle cx='17.6' cy='16.2' r='1.1'/>";
    private const string BrakePadsSvg = "<rect x='6' y='5' width='5' height='14' rx='1.5'/><rect x='13' y='5' width='5' height='14' rx='1.5'/>";
    private const string RotorSvg = "<circle cx='12' cy='12' r='8.5'/><circle cx='12' cy='12' r='3'/><circle cx='12' cy='5.7' r='0.8'/><circle cx='12' cy='18.3' r='0.8'/><circle cx='5.7' cy='12' r='0.8'/><circle cx='18.3' cy='12' r='0.8'/>";

    // ── Drivetrain ──
    private const string ChainSvg = "<rect x='3' y='8.5' width='8.5' height='7' rx='3.5'/><rect x='12.5' y='8.5' width='8.5' height='7' rx='3.5'/>";
    private const string CassetteSvg = "<circle cx='12' cy='12' r='8.5'/><circle cx='12' cy='12' r='5.5'/><circle cx='12' cy='12' r='2.5'/>";
    private const string ChainringSvg = "<circle cx='12' cy='12' r='7.2' stroke-dasharray='1.5 1.8'/><circle cx='12' cy='12' r='2.4'/><path d='M12 9.6V5.5M12 14.4v4.1M9.6 12H5.5M14.4 12h4.1'/>";
    private const string CranksetSvg = "<circle cx='10' cy='12' r='5.5' stroke-dasharray='1.4 1.7'/><circle cx='10' cy='12' r='2'/><path d='M10 12l8.5 4.5'/><rect x='17.6' y='15' width='3.6' height='4' rx='0.8'/>";
    private const string FrontDerailleurSvg = "<path d='M8 5v9a2 2 0 0 0 2 2h3a2 2 0 0 0 2-2V5'/><path d='M15 9h5'/><path d='M11 16v4'/>";
    private const string RearDerailleurSvg = "<path d='M7 4v2.5'/><circle cx='7' cy='9' r='2.3'/><circle cx='7' cy='9' r='0.6'/><path d='M9 9.5l3.5 7'/><circle cx='11.5' cy='18.5' r='2.3'/><circle cx='11.5' cy='18.5' r='0.6'/>";
    private const string ShifterSvg = "<path d='M4 7h3.5'/><circle cx='6' cy='7' r='1'/><rect x='8.5' y='9' width='7' height='6' rx='1.5'/><path d='M15.5 11l3.5-1.5M15.5 13.5l3.5 1.5'/>";
    private const string PulleySvg = "<circle cx='12' cy='12' r='6.5' stroke-dasharray='1.3 1.5'/><circle cx='12' cy='12' r='2'/><circle cx='12' cy='12' r='0.6'/>";
    private const string ChainguideSvg = "<rect x='6' y='6' width='12' height='12' rx='2'/><circle cx='12' cy='12' r='3'/><path d='M12 4v2M12 18v2'/>";
    private const string SprocketSvg = "<circle cx='12' cy='12' r='6.5'/><path d='M12 8.6l3 1.7v3.4l-3 1.7-3-1.7v-3.4z'/><path d='M12 3v2.5M12 18.5V21M4 12h2.5M17.5 12H20'/>";

    // ── Cockpit ──
    private const string HandlebarSvg = "<path d='M4 8h16M7 8v4a3 3 0 0 0 3 3M17 8v4a3 3 0 0 1-3 3'/>";
    private const string BarTapeSvg = "<rect x='3' y='9' width='18' height='6' rx='3'/><path d='M8 9l-2 6M12 9l-2 6M16 9l-2 6'/>";
    private const string GripsSvg = "<rect x='4' y='9' width='15' height='6' rx='3'/><path d='M8 9v6M11 9v6M14 9v6'/>";
    private const string StemSvg = "<path d='M6 13h8.5'/><circle cx='6' cy='13' r='2.6'/><rect x='14' y='8' width='5' height='8' rx='1.2'/><circle cx='16.5' cy='10.5' r='0.55'/><circle cx='16.5' cy='13.5' r='0.55'/>";

    // ── Wheels ──
    private const string WheelSvg = "<circle cx='12' cy='12' r='8.5'/><circle cx='12' cy='12' r='2'/><path d='M12 3.5V20.5M3.5 12h17M6 6l12 12M18 6L6 18'/>";
    private const string TireSvg = "<circle cx='12' cy='12' r='8.5'/><circle cx='12' cy='12' r='5'/>";
    private const string TireInsertSvg = "<circle cx='12' cy='12' r='8.5'/><circle cx='12' cy='12' r='5' stroke-dasharray='2.2 2.2'/>";
    private const string HubSvg = "<rect x='7' y='9' width='10' height='6' rx='2.5'/><path d='M3 12h4M17 12h4'/><path d='M8.5 9l-1-2M15.5 9l1-2M8.5 15l-1 2M15.5 15l1 2'/>";
    private const string SpokesSvg = "<circle cx='12' cy='12' r='2'/><path d='M12 3v7M12 14v7M3 12h7M14 12h7M5.6 5.6l4.4 4.4M14 14l4.4 4.4M18.4 5.6L14 10M10 14l-4.4 4.4'/>";
    private const string RimTapeSvg = "<circle cx='12' cy='12' r='6'/><circle cx='12' cy='12' r='2'/><path d='M18 12a6 6 0 0 0-6-6'/>";
    private const string InnerTubeSvg = "<circle cx='12' cy='13' r='7'/><rect x='11' y='2' width='2' height='4' rx='0.5'/>";
    private const string DropSvg = "<path d='M12 3s6 7 6 11a6 6 0 0 1-12 0c0-4 6-11 6-11z'/>";
    private const string ThruAxleSvg = "<path d='M3 12h16'/><circle cx='4' cy='12' r='1.6'/><path d='M17 9.5v5M19.5 10.5v3'/>";

    // ── Structure ──
    private const string FrameSvg = "<path d='M4 18h7l-2-11h9l-7 11'/><path d='M9 7l-5 11'/><path d='M18 7l1.2-1.6'/>";
    private const string ForkSvg = "<path d='M12 4v6m0 0l-4.5 9M12 10l4.5 9'/>";
    private const string HeadsetSvg = "<ellipse cx='12' cy='7' rx='7' ry='2'/><ellipse cx='12' cy='12' rx='7' ry='2'/><ellipse cx='12' cy='17' rx='7' ry='2'/>";
    private const string BottomBracketSvg = "<path d='M3 12h18'/><circle cx='8' cy='12' r='2.6'/><circle cx='16' cy='12' r='2.6'/>";
    private const string BearingSvg = "<circle cx='12' cy='12' r='8'/><circle cx='12' cy='12' r='3'/><circle cx='12' cy='5.5' r='1'/><circle cx='12' cy='18.5' r='1'/><circle cx='5.5' cy='12' r='1'/><circle cx='18.5' cy='12' r='1'/>";
    private const string BoltsSvg = "<circle cx='12' cy='8' r='3.5'/><path d='M10 6.5l4 3M14 6.5l-4 3M12 11.5V20'/>";
    private const string PedalsSvg = "<rect x='6' y='9.5' width='9' height='5' rx='1'/><path d='M15 12h5'/><path d='M8 9.5v-1.3M11 9.5v-1.3M8 14.5v1.3M11 14.5v1.3'/>";
    private const string SaddleSvg = "<path d='M4 10c5-3 11-3 16 0-2 4-6 5-8 5s-6-1-8-5z'/><path d='M12 15v5'/>";
    private const string SeatpostSvg = "<rect x='10' y='4' width='4' height='16' rx='1.5'/>";

    // ── Suspension ──
    private const string SuspensionForkSvg = "<path d='M8 4h8M8 4v15M16 4v15'/><rect x='6.5' y='12' width='3' height='7' rx='1'/><rect x='14.5' y='12' width='3' height='7' rx='1'/>";
    private const string ShockSvg = "<rect x='9' y='3' width='6' height='18' rx='3'/><path d='M9 8h6M9 12h6M9 16h6'/>";
    private const string DropperSeatpostSvg = "<rect x='10' y='3' width='4' height='13' rx='1.5'/><path d='M9 18l3 3 3-3'/>";
    private const string SuspensionSeatpostSvg = "<rect x='10' y='3' width='4' height='5' rx='1'/><path d='M10 8l4 2-4 2 4 2-4 2'/><rect x='10' y='18' width='4' height='3' rx='1'/>";

    // ── Cables ──
    private const string CableSvg = "<path d='M4 6q9 0 9 6t7 6'/><circle cx='4' cy='6' r='1.4'/>";
    private const string HydraulicLinesSvg = "<path d='M5 5q8 1 8 7t6 7'/><circle cx='5' cy='5' r='1.4'/><circle cx='19' cy='19' r='1.4'/>";

    // ── Electric ──
    private const string BatterySvg = "<rect x='4' y='8' width='15' height='8' rx='1.5'/><path d='M20 10.5v3M7 12h4'/>";
    private const string MotorSvg = "<circle cx='12' cy='12' r='7.2' stroke-dasharray='1.5 1.8'/><path d='M12.5 7.5l-3 5.5h3l-1 4 3.5-6h-3z'/>";

    // ── Indoor ──
    private const string TrainerSvg = "<circle cx='12' cy='9' r='4'/><path d='M6 20l4-8M18 20l-4-8M8 20h8'/>";
    private const string IndoorBikeSvg = "<circle cx='7' cy='16' r='3'/><circle cx='17' cy='16' r='2.5'/><path d='M7 16l3-7h5M10 9l5 7M8 9h2'/>";
    private const string FanSvg = "<circle cx='12' cy='12' r='2'/><path d='M12 10c-2.5-3.5.5-6.5 2.5-5.5s.5 5-2.5 5.5M14 12c3.5-2.5 6.5.5 5.5 2.5s-5 .5-5.5-2.5M12 14c2.5 3.5-.5 6.5-2.5 5.5s-.5-5 2.5-5.5M10 12c-3.5 2.5-6.5-.5-5.5-2.5s5-.5 5.5 2.5'/>";
    private const string MatSvg = "<rect x='4' y='9' width='16' height='6' rx='1'/><circle cx='7' cy='12' r='2.2'/>";
    private const string RiserSvg = "<path d='M6 16h12l-1.5-5h-9z'/><path d='M9 11V8h6v3'/>";

    // ── Accessories ──
    private const string ComputerSvg = "<rect x='7' y='4' width='10' height='16' rx='2'/><path d='M9.5 8h5'/>";
    private const string LightsSvg = "<circle cx='9' cy='12' r='4'/><path d='M13 12h7M14 9l6-2M14 15l6 2'/>";
    private const string LockSvg = "<rect x='6' y='11' width='12' height='9' rx='2'/><path d='M8.5 11V8a3.5 3.5 0 0 1 7 0v3'/>";
    private const string PumpSvg = "<rect x='9' y='6' width='6' height='14' rx='1'/><path d='M12 6V2M9 4h6M15 17h5'/>";
    private const string RackSvg = "<path d='M4 7h16M6 7v10M18 7v10M4 17h16'/>";
    private const string BottleSvg = "<path d='M10 3h4v2l1 2v11a1 1 0 0 1-1 1h-4a1 1 0 0 1-1-1V7l1-2z'/><path d='M9 11h6'/>";
    private const string BellHornSvg = "<path d='M12 4a5 5 0 0 0-5 5c0 5-2 6-2 6h14s-2-1-2-6a5 5 0 0 0-5-5z'/><path d='M10.5 19a1.5 1.5 0 0 0 3 0'/>";
    private const string FendersSvg = "<path d='M4 16a8 8 0 0 1 16 0'/><path d='M4 16v2M20 16v2'/>";
    private const string KickstandSvg = "<path d='M11 4v11M11 15l-4 5M11 15l3 5M7 20h8'/>";
    private const string ToolsetSvg = "<path d='M14.5 4a4 4 0 0 0-5 5l-5.5 5.5 2.5 2.5 5.5-5.5a4 4 0 0 0 5-5l-2.6 2.6-2-2z'/>";
    private const string ApparelSvg = "<path d='M8 4L4 7l2 3 2-1v8h8v-8l2 1 2-3-4-3h-3a2 2 0 0 1-4 0z'/>";
    private const string TagSvg = "<path d='M4 4h7l9 9-7 7-9-9z'/><circle cx='8' cy='8' r='1.4'/>";
    private const string OtherSvg = "<circle cx='6' cy='12' r='1.5'/><circle cx='12' cy='12' r='1.5'/><circle cx='18' cy='12' r='1.5'/>";
}
