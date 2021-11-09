using AutoMapper;
using FluentResults;
using KOMTracker.API.DAL;
using KOMTracker.API.DAL.Repositories;
using KOMTracker.API.Models.Athlete;
using KOMTracker.API.Models.Segment;
using KOMTracker.API.Models.Token;
using Strava.API.Client.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiModel = Strava.API.Client.Model;

namespace KOMTracker.API.Infrastructure.Services
{
    public class AthleteService : IAthleteService
    {
        private readonly IMapper _mapper;
        private readonly IKOMUnitOfWork _komUoW;
        private readonly ITokenService _tokenService;
        private readonly IAthleteApi _athleteApi;

        public AthleteService(IMapper mapper, IKOMUnitOfWork komUoW, ITokenService tokenService, IAthleteApi athleteApi)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _athleteApi = athleteApi ?? throw new ArgumentNullException(nameof(athleteApi));
        }

        public Task<bool> IsAthleteExistsAsync(int athleteId)
        {
            return _komUoW
                .GetRepository<IAthleteRepository>()
                .IsAthleteExistsAsync(athleteId);
        }

        public Task AddOrUpdateAthleteAsync(AthleteModel athlete)
        {
            return _komUoW
                .GetRepository<IAthleteRepository>()
                .AddOrUpdateAthleteAsync(athlete);
        }

        public Task AddOrUpdateTokenAsync(TokenModel token)
        {
            return _komUoW
                .GetRepository<IAthleteRepository>()
                .AddOrUpdateTokenAsync(token);
        }

        public async Task<IEnumerable<AthleteModel>> GetAllAthletesAsync()
        {
            return await _komUoW
                .GetRepository<IAthleteRepository>()
                .GetAllAthletesAsync();
        }

        public async Task<TokenModel> GetValidTokenAsync(int athleteId)
        {
            var token = await GetTokenAsync(athleteId);

            if (token == null)
            {
                // TODO: Decativate athlete 
                throw new Exception($"{nameof(GetValidTokenAsync)} no token in DB!");
            }

            if (IsValidToken(token))
            {
                return token;
            }

            var refreshTokenResult = await _tokenService.RefreshAsync(token);

            if (refreshTokenResult.IsSuccess)
            {
                var newToken = refreshTokenResult.Value;
                await AddOrUpdateTokenAsync(newToken);
                return newToken;
            }

            // TODO: Deactivate athlete on Invalid refresh token
            throw new Exception($"{nameof(GetValidTokenAsync)} falied!");
        }

        public async Task<IEnumerable<(SegmentEffortModel, SegmentModel)>> GetAthleteKomsAsync(int athleteId)
        {
            var token = await GetValidTokenAsync(athleteId);

            var getKomsRes = await _athleteApi.GetKomsAsync(athleteId, token.AccessToken);
            // TODO: retry on Unauthorized 
            if (getKomsRes.IsFailed)
            {
                throw new Exception($"{nameof(GetAthleteKomsAsync)} cannot get KOMs!");
            }

            var koms = getKomsRes.Value;

            return koms
                .Where(x => !x.Segment.Private)
                .Select(x => (
                    _mapper.Map<SegmentEffortModel>(x),
                    _mapper.Map<SegmentModel>(x.Segment)
                ))
                .ToList();
        }

        protected Task<TokenModel> GetTokenAsync(int athleteId)
        {
            return _komUoW
                .GetRepository<IAthleteRepository>()
                .GetTokenAsync(athleteId);
        }

        protected bool IsValidToken(TokenModel token)
        {
            return token.ExpiresAt > DateTime.UtcNow;
        }
    }
}
