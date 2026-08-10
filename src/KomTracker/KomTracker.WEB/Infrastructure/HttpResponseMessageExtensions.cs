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
