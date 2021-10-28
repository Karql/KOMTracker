using AutoMapper;
using KOMTracker.API.DAL;
using KOMTracker.API.Models.Identity;
using KOMTracker.API.Models.Strava;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KOMTracker.API.Infrastructure.Services
{
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

        public IMapper Mapper => _mapper;

        public async Task Connect(string code, string scope)
        {
            // TODO: transaction
            VerifyScope(scope);

            var exchangeResult = await _tokenService.ExchangeTokenAsync(code, scope);

            if (!exchangeResult.IsSuccess)
            {
                // TODO: Error
                return;
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
        }

        protected bool VerifyScope(string scope)
        {
            var scopes = scope?.Split(",") ?? Enumerable.Empty<string>();

            return REQUIRED_SCOPES.All(x => scopes.Contains(scope));
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
}
