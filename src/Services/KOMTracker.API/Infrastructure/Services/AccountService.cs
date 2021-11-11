using AutoMapper;
using FluentResults;
using KOMTracker.API.DAL;
using KOMTracker.API.Models.Account.Error;
using KOMTracker.API.Models.Athlete;
using KOMTracker.API.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KOMTracker.API.Infrastructure.Services;

public class AccountService : IAccountService
{
    private static readonly HashSet<string> REQUIRED_SCOPES = new()
    {
        "read",
        "activity:read",
        "profile:read_all"
    };

    private readonly IMapper _mapper;
    private readonly IKOMUnitOfWork _komUoW;
    private readonly ITokenService _tokenService;
    private readonly IAthleteService _athleteService;
    private readonly UserManager<UserModel> _userManager;

    public AccountService(IMapper mapper, IKOMUnitOfWork komUoW, ITokenService tokenService, IAthleteService athleteService, UserManager<UserModel> userManager)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _athleteService = athleteService ?? throw new ArgumentNullException(nameof(athleteService));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    public async Task<Result> Connect(string code, string scope)
    {
        // TODO: transaction
        if (!VerifyRequiredScope(scope))
        {
            return Result.Fail(new ConnectError(ConnectError.NoRequiredScope));
        }

        var exchangeResult = await _tokenService.ExchangeAsync(code, scope);

        if (!exchangeResult.IsSuccess)
        {
            return Result.Fail(new ConnectError(ConnectError.InvalidCode));
        }

        var (athlete, token) = exchangeResult.Value;

        await _athleteService.AddOrUpdateAthleteAsync(athlete);
        await _athleteService.AddOrUpdateTokenAsync(token);

        if (!await IsUserExistsAsync(athlete.AthleteId))
        {
            await AddUser(athlete);
        }

        await _komUoW.SaveChangesAsync();

        // TODO: Login

        return Result.Ok();
    }

    protected bool VerifyRequiredScope(string scope)
    {
        var scopes = scope?.Split(",") ?? Enumerable.Empty<string>();

        return REQUIRED_SCOPES.All(x => scopes.Contains(x));
    }

    protected Task<bool> IsUserExistsAsync(int athleteId)
    {
        return _userManager.Users.AnyAsync(x => x.AthleteId == athleteId);
    }

    protected Task AddUser(AthleteModel athlete)
    {
        return _userManager.CreateAsync(new UserModel
        {
            AthleteId = athlete.AthleteId,
            UserName = athlete.Username
        });
    }
}
