using KomTracker.Application.Shared.Models.Segment;

namespace KomTracker.Application.Shared.Helpers;

public static class GeoHelper
{
    /// <summary>
    /// Initial great-circle bearing from point 1 to point 2, in degrees, normalized to [0, 360).
    /// 0 = north, 90 = east, 180 = south, 270 = west.
    /// </summary>
    public static double GetBearing(double lat1, double lon1, double lat2, double lon2)
    {
        var phi1 = DegToRad(lat1);
        var phi2 = DegToRad(lat2);
        var deltaLambda = DegToRad(lon2 - lon1);

        var y = Math.Sin(deltaLambda) * Math.Cos(phi2);
        var x = (Math.Cos(phi1) * Math.Sin(phi2)) - (Math.Sin(phi1) * Math.Cos(phi2) * Math.Cos(deltaLambda));

        var bearing = RadToDeg(Math.Atan2(y, x));
        return (bearing + 360) % 360;
    }

    /// <summary>Bucket a 0-360 bearing into an 8-point compass direction (N covers 337.5-22.5).</summary>
    public static CompassDirection GetCompassDirection(double bearing)
    {
        var normalized = ((bearing % 360) + 360) % 360;
        var index = (int)Math.Floor((normalized / 45.0) + 0.5) % 8;
        return (CompassDirection)index;
    }

    public static string GetCompassDirectionText(CompassDirection direction) => direction.ToString();

    private static double DegToRad(double deg) => deg * Math.PI / 180.0;

    private static double RadToDeg(double rad) => rad * 180.0 / Math.PI;
}
