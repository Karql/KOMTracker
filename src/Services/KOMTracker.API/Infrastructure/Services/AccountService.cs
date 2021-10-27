using AutoMapper;
using KOMTracker.API.DAL;
using KOMTracker.API.Models.Identity;
using KOMTracker.API.Models.Strava;
using Microsoft.AspNetCore.Identity;
using Strava.API.Client.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KOMTracker.API.Infrastructure.Services
{
    public class AccountService : IAccountService
    {
        private readonly IMapper _mapper;
        private readonly IKOMUnitOfWork _komUoW;
        private readonly ITokenApi _tokenApi;
        private readonly IAthleteService _athleteService;
        private readonly UserManager<UserModel> _userManager;

        public AccountService(IMapper mapper, IKOMUnitOfWork komUoW, ITokenApi tokenApi, IAthleteService athleteService, UserManager<UserModel> userManager)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
            _tokenApi = tokenApi ?? throw new ArgumentNullException(nameof(tokenApi));
            _athleteService = athleteService ?? throw new ArgumentNullException(nameof(athleteService));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public IMapper Mapper => _mapper;

        public async Task Connect(string code, string scope)
        {
            // TODO: transaction

            VerifyScope(scope);

            var (athlete, token) = await ExchangeTokenAsync(code, scope);

            await _athleteService.AddOrUpdateAthleteAsync(athlete);
            await _athleteService.AddOrUpdateTokenAsync(token);

            await _komUoW.SaveChangesAsync();
        }

        protected bool VerifyScope(string scope)
        {
            // TODO
            return true;
        }

        protected async Task<(AthleteModel, TokenModel)> ExchangeTokenAsync(string code, string scope)
        {
            var exchangeResult = await _tokenApi.ExchangeAsync(code);
            if (!exchangeResult.IsSuccess)
            {
                // TODO: 
                // - better error handling
                throw new Exception($"{nameof(_tokenApi.ExchangeAsync)} failed!");
            }

            var apiTokenWithAthlete = exchangeResult.Value;
            var athlete = _mapper.Map<AthleteModel>(apiTokenWithAthlete.Athlete);
            var token = _mapper.Map<TokenModel>(apiTokenWithAthlete);
            token.Scope = scope;

            return (athlete, token);
        }
    }
}
