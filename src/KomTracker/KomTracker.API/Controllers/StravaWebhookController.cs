using System.Text.Json;
using KomTracker.API.Infrastructure.Jobs;
using KomTracker.API.Shared.ViewModels.Strava;
using KomTracker.Application.Commands.Strava;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Strava.API.Client.Configurations;

namespace KomTracker.API.Controllers;

/// <summary>
/// Public Strava webhook callback (D-12 inbox). Unauthenticated — Strava calls it — so the private <c>{secret}</c>
/// in the path is the gate (Strava does not sign its requests). GET is the subscription-validation handshake; POST
/// persists each event raw. No processing here; a later worker drains stored events.
/// </summary>
[Route("strava/callback/{secret}")]
[ApiController]
public class StravaWebhookController : BaseApiController<StravaWebhookController>
{
    private StravaApiClientConfiguration Config =>
        HttpContext.RequestServices.GetRequiredService<StravaApiClientConfiguration>();

    /// <summary>Subscription validation: echo hub.challenge when the verify token matches (per Strava's docs).</summary>
    [HttpGet]
    public IActionResult Validate(
        [FromRoute] string secret,
        [FromQuery(Name = "hub.mode")] string? mode,
        [FromQuery(Name = "hub.challenge")] string? challenge,
        [FromQuery(Name = "hub.verify_token")] string? verifyToken)
    {
        if (secret != Config.WebhookSecret)
        {
            return NotFound();
        }

        if (verifyToken != Config.WebhookVerifyToken)
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        return Ok(new Dictionary<string, string?> { ["hub.challenge"] = challenge });
    }

    /// <summary>Event ingestion: persist raw and return 200 fast (Strava requires a response within ~2s).</summary>
    [HttpPost]
    public async Task<IActionResult> Receive([FromRoute] string secret, [FromBody] StravaWebhookEventViewModel model)
    {
        if (secret != Config.WebhookSecret)
        {
            return NotFound();
        }

        // TODO(D-12-4): as an extra guard, also verify model.SubscriptionId == the configured push subscription id
        // once it's known (available only after the subscription is created). Not enforced now.

        string? updates = null;
        if (model.Updates is JsonElement el && el.ValueKind != JsonValueKind.Null && el.ValueKind != JsonValueKind.Undefined)
        {
            updates = el.GetRawText();
        }

        await _mediator.Send(new StoreStravaWebhookEventCommand
        {
            ObjectType = model.ObjectType ?? string.Empty,
            ObjectId = model.ObjectId,
            AspectType = model.AspectType ?? string.Empty,
            Updates = updates,
            OwnerId = model.OwnerId,
            SubscriptionId = model.SubscriptionId,
            EventTime = model.EventTime
        });

        await TriggerProcessingAsync();

        return Ok();
    }

    // Best-effort low-latency kick of the drain job. Unconditional (no "skip if running" guard): with
    // [DisallowConcurrentExecution] a trigger fired mid-run is blocked and runs right after — never dropped — so a
    // burst collapses without delaying events. The hourly job is the backstop if this throws or the job is disabled.
    private async Task TriggerProcessingAsync()
    {
        try
        {
            var schedulerFactory = HttpContext.RequestServices.GetRequiredService<ISchedulerFactory>();
            var scheduler = await schedulerFactory.GetScheduler();
            await scheduler.TriggerJob(ProcessStravaWebhookEventsJob.Key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not trigger {job} on webhook receipt.", nameof(ProcessStravaWebhookEventsJob));
        }
    }
}
