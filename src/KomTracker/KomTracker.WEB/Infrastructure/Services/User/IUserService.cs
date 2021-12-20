using KomTracker.WEB.Models.User;
using System.Security.Claims;

namespace KomTracker.WEB.Infrastructure.Services.User;

public interface IUserService
{
    public Task<UserModel> GetCurrentUser();
}
