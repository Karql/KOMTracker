using KomTracker.Application.Interfaces.Services.Identity;
using KomTracker.Domain.Entities.Athlete;
using KomTracker.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Infrastructure.Identity.Services;

public class UserService : IUserService
{
    private readonly UserManager<UserEntity> _userManager;

    public UserService(UserManager<UserEntity> userManager)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    public Task<bool> IsUserExistsAsync(int athleteId)
    {
        return _userManager.Users.AnyAsync(x => x.AthleteId == athleteId);
    }

    public Task AddUserAsync(AthleteEntity athlete)
    {
        return _userManager.CreateAsync(new UserEntity
        {
            AthleteId = athlete.AthleteId,
            UserName = athlete.Username
        });
    }
}
