﻿using AutoMapper;
using FluentResults;
using KomTracker.Application.Interfaces.Services.Identity;
using KomTracker.Application.Models.Identity;
using KomTracker.Domain.Entities.Athlete;
using KomTracker.Infrastructure.Identity.Configurations;
using KomTracker.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KomTracker.Infrastructure.Shared.Identity.Constants;

namespace KomTracker.Infrastructure.Identity.Services;

public class UserService : IUserService
{
    private readonly IMapper _mapper;
    private readonly UserManager<UserEntity> _userManager;
    private readonly IdentityConfiguration _identityConfiguration;

    public UserService(IMapper mapper, UserManager<UserEntity> userManager, IdentityConfiguration identityConfiguration)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _identityConfiguration = identityConfiguration ?? throw new ArgumentNullException(nameof(identityConfiguration));
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

    public async Task<Result<string>> GenerateChangeEmailUrlAsync(int athleteId, string newEmail)
    {
        var user = await GetUserByAthleteIdAsync(athleteId);

        if (user == null)
        {
            return Result.Fail(new GenerateChangeEmailUrlError(GenerateChangeEmailUrlError.UserNotFound));
        }

        var token = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);

        return Result.Ok(GetConfirmChangeEmailUrl(athleteId, newEmail, token));
    }

    public async Task<Result> ConfirmEmailChangeAsync(int athleteId, string newEmail, string token)
    {
        var user = await GetUserByAthleteIdAsync(athleteId);

        if (user == null)
        {
            return Result.Fail(new ConfirmEmailChangeError(ConfirmEmailChangeError.UserNotFound));
        }

        var res = await _userManager.ChangeEmailAsync(user, newEmail, token);

        if (res.Succeeded)
        {
            return Result.Ok();
        }

        var error = res.Errors.FirstOrDefault()!;

        return Result.Fail(new ConfirmEmailChangeError(ConfirmEmailChangeError.ChangeEmailFailed) { ChangeEmailFailedMsg = error.Description });
    }

    private Task<UserEntity> GetUserByAthleteIdAsync(int athleteId)
    {
        return _userManager.Users.FirstOrDefaultAsync(x => x.AthleteId == athleteId);
    }

    private string GetConfirmChangeEmailUrl(int athleteId, string newEmail, string token)
    {
        var tokenEncoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        return $"{_identityConfiguration.IdentityUrl}{ProtocolRoutePaths.ConfirmEmailChange}?athlete_id={athleteId}&new_email={newEmail}&token={tokenEncoded}";
    }
}
