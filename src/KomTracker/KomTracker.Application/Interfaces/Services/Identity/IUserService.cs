using FluentResults;
using KomTracker.Application.Models.Identity;
using KomTracker.Domain.Entities.Athlete;

namespace KomTracker.Application.Interfaces.Services.Identity;

public interface IUserService
{
    Task<bool> IsUserExistsAsync(int athleteId);
    Task<UserModel?> GetUserAsync(int athleteId);
    Task AddUserAsync(AthleteEntity athlete);
    Task<Result<string>> GenerateChangeEmailUrlAsync(int athleteId, string newEmail);
    Task<Result> ConfirmEmailChangeAsync(int athleteId, string newEmail, string token);
    Task<Result> UpdateCurrencyAsync(int athleteId, string currency);

    /// <summary>Stamps the user's last sign-in time (now, UTC) — called when the Strava login completes.</summary>
    Task RecordLoginAsync(int athleteId);
}

public class GenerateChangeEmailUrlError : FluentResults.Error
{
    public const string UserNotFound = "UserNotFound";    

    public GenerateChangeEmailUrlError(string message)
        : base(message)
    {
    }
}

public class ConfirmEmailChangeError : FluentResults.Error
{
    public const string UserNotFound = "UserNotFound";
    public const string ChangeEmailFailed = "ChangeEmailFailed";

    public string? ChangeEmailFailedMsg { get; set; }

    public ConfirmEmailChangeError(string message)
        : base(message)
    {
    }
}