using AutoMapper;
using KomTracker.Application.Interfaces.Services.Identity;
using KomTracker.Application.Models.Identity;
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
    private readonly IMapper _mapper;
    private readonly UserManager<UserEntity> _userManager;

    public UserService(IMapper mapper, UserManager<UserEntity> userManager)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    public Task<bool> IsUserExistsAsync(int athleteId)
    {
        return _userManager.Users.AnyAsync(x => x.AthleteId == athleteId);
    }

    public async Task<UserModel?> GetUserAsync(int athleteId)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.AthleteId == athleteId);

        if (user != null)
        {
            return _mapper.Map<UserModel>(user);
        }

        return null;
    }

    public Task AddUserAsync(AthleteEntity athlete)
    {
        return _userManager.CreateAsync(new UserEntity
        {
            AthleteId = athlete.AthleteId,
            UserName = athlete.Username
        });
    }

    public async Task ChangeUserMailAsync(int athleteId, string newEmail)
    {
        var user = await GetUserByAthleteIdAsync(athleteId);

        // TODO: check mail already exists

        var token = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);

        await _userManager.ChangeEmailAsync(user, newEmail, token);
    }

    private Task<UserEntity> GetUserByAthleteIdAsync(int athleteId)
    {
        return _userManager.Users.FirstOrDefaultAsync(x => x.AthleteId == athleteId);
    }
}
