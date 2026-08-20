using System.Text.Json.Serialization;

namespace KomTracker.Domain.Entities.Component;

/// <summary>
/// Component category (CONCEPT §14 seed set). Persisted and serialized by name (string) —
/// see ComponentEntityTypeConfiguration. Renaming a member requires a data migration (stored value = member name).
/// Front/rear/left/right is NOT here (that's Installation.Position) — the only kept split is the two derailleurs.
/// UI grouping + ordering come from <see cref="ComponentCategoryMetadata"/>. <see cref="Other"/> is the fallback.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ComponentCategory
{
    // Brakes
    Brake,
    BrakeCaliper,
    BrakeLever,
    BrakePads,
    BrakeRotor,

    // Drivetrain
    Chain,
    Cassette,
    Chainring,
    Crankset,
    FrontDerailleur,
    RearDerailleur,
    Shifter,
    Pulley,
    Chainguide,
    Sprocket,

    // Cockpit
    Handlebar,
    BarTape,
    Grips,
    Stem,

    // Wheels
    Wheel,
    Tire,
    TireInsert,
    Hub,
    Spokes,
    RimTape,
    InnerTube,
    TubelessSealant,
    ThruAxle,

    // Structure
    Frame,
    Fork,
    Headset,
    BottomBracket,
    Bearing,
    Bolts,
    Pedals,
    Saddle,
    Seatpost,

    // Suspension
    SuspensionFork,
    RearShock,
    DropperSeatpost,
    SuspensionSeatpost,

    // Cables
    Cable,
    HydraulicLines,

    // Electric
    Battery,
    Motor,

    // Indoor
    Trainer,
    IndoorBike,
    Fan,
    Mat,
    Riser,

    // Accessories
    Computer,
    Lights,
    Lock,
    Pump,
    Rack,
    Bottle,
    BellHorn,
    Fenders,
    Kickstand,
    Toolset,
    Apparel,
    Accessories,
    Other
}
