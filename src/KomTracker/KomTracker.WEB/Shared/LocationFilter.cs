namespace KomTracker.WEB.Shared;

/// <summary>A picked map point + radius (km) used to filter segments by proximity of their start.</summary>
public record LocationFilter(double Lat, double Lng, double RadiusKm);
