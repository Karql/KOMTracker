using FluentResults;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence.Repositories;

namespace KomTracker.Application.Commands.Installation;

/// <summary>
/// The D-7 linkage invariant for a Tracked placement (2b-ii). A component's active Tracked installations are
/// homogeneous: either into exactly one parent component, or onto one-or-more <b>distinct</b> bikes — never mixed,
/// never two parent components (D-2b2-2). Component-in-component is exclusive and limited to a single level, with
/// no cycles (D-2b2-3). Manual (historical) rows are exempt and never pass through here.
/// </summary>
internal static class InstallationInvariant
{
    /// <summary>
    /// Validates installing/moving component <paramref name="componentId"/> onto <paramref name="bikeId"/> XOR into
    /// <paramref name="parentComponentId"/> (exactly one set — the caller's validator guarantees this).
    /// <paramref name="excludeInstallationId"/> skips a row being moved/updated so it never conflicts with itself.
    /// </summary>
    public static async Task<Result> CheckAsync(
        IInstallationRepository installationRepo,
        int componentId,
        int? bikeId,
        int? parentComponentId,
        int? excludeInstallationId = null)
    {
        var active = (await installationRepo.GetActiveTrackedInstallationsByComponentAsync(componentId))
            .Where(i => excludeInstallationId is null || i.Id != excludeInstallationId)
            .ToList();

        if (bikeId is int targetBike)
        {
            // Homogeneity: can't be on a bike while it's sitting inside a parent component.
            if (active.Any(i => i.ParentComponentId is not null))
            {
                return Result.Fail(new ConflictError(
                    "Component is inside another component — remove it from there first."));
            }

            // Distinct bikes only — no duplicate placement on the same bike.
            if (active.Any(i => i.BikeId == targetBike))
            {
                return Result.Fail(new ConflictError("Component is already installed on this bike."));
            }

            // Multi-bike onto a different bike is allowed.
            return Result.Ok();
        }

        var targetParent = parentComponentId!.Value;

        if (targetParent == componentId)
        {
            return Result.Fail(new ConflictError("A component cannot be installed into itself."));
        }

        // Component-in-component is exclusive: the component must have no current active placements.
        if (active.Count > 0)
        {
            return Result.Fail(new ConflictError(
                "Component is currently installed — remove it before putting it into another component."));
        }

        var parentActive = (await installationRepo.GetActiveTrackedInstallationsByComponentAsync(targetParent)).ToList();

        // One level (target side): the parent must not itself be inside another component.
        if (parentActive.Any(i => i.ParentComponentId is not null))
        {
            return Result.Fail(new ConflictError(
                "Nesting is limited to one level — the target component is itself installed inside another component."));
        }

        // Cycle guard (defensive; one-level already precludes it): parent must not be a child of this component.
        if (parentActive.Any(i => i.ParentComponentId == componentId))
        {
            return Result.Fail(new ConflictError("That would create a cycle."));
        }

        // One level (source side): the component being installed must not already contain other components.
        var ownChildren = await installationRepo.GetActiveChildrenByParentComponentsAsync(new[] { componentId });
        if (ownChildren.Any())
        {
            return Result.Fail(new ConflictError(
                "Nesting is limited to one level — this component already contains other components."));
        }

        return Result.Ok();
    }
}
