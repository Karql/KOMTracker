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

    /// <summary>Great-circle (haversine) distance between two points, in kilometers.</summary>
    public static double GetDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusKm = 6371.0;

        var phi1 = DegToRad(lat1);
        var phi2 = DegToRad(lat2);
        var deltaPhi = DegToRad(lat2 - lat1);
        var deltaLambda = DegToRad(lon2 - lon1);

        var a = (Math.Sin(deltaPhi / 2) * Math.Sin(deltaPhi / 2))
            + (Math.Cos(phi1) * Math.Cos(phi2) * Math.Sin(deltaLambda / 2) * Math.Sin(deltaLambda / 2));
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }

    private static double DegToRad(double deg) => deg * Math.PI / 180.0;

    private static double RadToDeg(double rad) => rad * 180.0 / Math.PI;
}
