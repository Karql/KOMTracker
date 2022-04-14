using KomTracker.API.Shared.Models.User;

namespace KomTracker.WEB.Infrastructure.Services.User;

public interface IUserService
{
    public Task<UserModel> GetCurrentUser();
}
