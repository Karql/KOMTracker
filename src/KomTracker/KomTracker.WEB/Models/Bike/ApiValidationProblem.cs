namespace KomTracker.WEB.Models.Bike;

/// <summary>Minimal client-side shape for RFC 7807 (validation) problem+json responses.</summary>
public class ApiValidationProblem
{
    public Dictionary<string, string[]>? Errors { get; set; }
    public string? Detail { get; set; }
    public string? Title { get; set; }
}
