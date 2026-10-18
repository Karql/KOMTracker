using System.Net;
using System.Net.Http.Json;
using KomTracker.WEB.Models.Bike;
using MudBlazor;

namespace KomTracker.WEB.Infrastructure;

public static class HttpResponseMessageExtensions
{
    /// <summary>
    /// Reads an RFC 7807 problem+json body and surfaces its messages via the snackbar.
    /// 422 → per-field validation messages; otherwise the problem detail/title or a status fallback.
    /// </summary>
    public static async Task ShowProblemAsync(this HttpResponseMessage response, ISnackbar snackbar)
    {
        try
        {
            if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
            {
                var problem = await response.Content.ReadFromJsonAsync<ApiValidationProblem>();
                if (problem?.Errors is { Count: > 0 })
                {
                    foreach (var message in problem.Errors.Values.SelectMany(m => m))
                    {
                        snackbar.Add(message, Severity.Error);
                    }
                    return;
                }
            }

            var detail = await TryReadDetailAsync(response);
            snackbar.Add(detail ?? $"Request failed ({(int)response.StatusCode}).", Severity.Error);
        }
        catch
        {
            snackbar.Add($"Request failed ({(int)response.StatusCode}).", Severity.Error);
        }
    }

    /// <summary>
    /// True when a 409 is the "covered by a service" delete guard (D-SRV-6) — a block no "delete anyway" can lift, so
    /// the caller should just show the reason. Matches the marker in the API's <c>DeleteBlockMessages</c>.
    /// </summary>
    public static async Task<bool> IsServiceDeleteBlockAsync(this HttpResponseMessage response)
        => response.StatusCode == HttpStatusCode.Conflict
            && (await TryReadDetailAsync(response))?.Contains("covered by a service", StringComparison.OrdinalIgnoreCase) == true;

    private static async Task<string?> TryReadDetailAsync(HttpResponseMessage response)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ApiValidationProblem>();
            return problem?.Detail ?? problem?.Title;
        }
        catch
        {
            return null;
        }
    }
}
