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
}

public class ChangeUserMailError : FluentResults.Error
{
    public const string UserNotFound = "UserNotFound";

    public ChangeUserMailError(string message)
        : base(message)
    {
    }
}