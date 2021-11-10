using AutoMapper;
using FluentResults;
using KOMTracker.API.DAL;
using KOMTracker.API.DAL.Repositories;
using KOMTracker.API.Models.Athlete;
using KOMTracker.API.Models.Athlete.Error;
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

        public async Task<Result<TokenModel>> GetValidTokenAsync(int athleteId)
        {
            var token = await GetTokenAsync(athleteId);

            if (token == null)
            {
                await DeactivateAthleteAsync(athleteId);
                return Result.Fail<TokenModel>(new GetValidTokenError(GetValidTokenError.NoTokenInDB));
            }

            if (IsValidToken(token))
            {
                return Result.Ok(token);
            }

            var refreshTokenResult = await _tokenService.RefreshAsync(token);

            if (refreshTokenResult.IsSuccess)
            {
                var newToken = refreshTokenResult.Value;
                await AddOrUpdateTokenAsync(newToken);
                return Result.Ok(newToken);
            }

            await DeactivateAthleteAsync(athleteId);
            return Result.Fail<TokenModel>(new GetValidTokenError(GetValidTokenError.RefreshFailed));
        }

        public async Task<Result<IEnumerable<(SegmentEffortModel, SegmentModel)>>> GetAthleteKomsAsync(int athleteId, string token)
        {
            var getKomsRes = await _athleteApi.GetKomsAsync(athleteId, token);
            
            if (getKomsRes.IsSuccess)
            {
                return Result.Ok(getKomsRes.Value
                    .Where(x => !x.Segment.Private)
                    .Select(x => (
                        _mapper.Map<SegmentEffortModel>(x),
                        _mapper.Map<SegmentModel>(x.Segment)
                    ))
                    .ToList()
                    .AsEnumerable()
                );
            }

            if (getKomsRes.HasError<ApiModel.Segment.Error.GetKomsError>(x => x.Message == ApiModel.Segment.Error.GetKomsError.Unauthorized))
            {
                return Result.Fail<IEnumerable<(SegmentEffortModel, SegmentModel)>>(new GetAthleteKomsError(GetAthleteKomsError.Unauthorized));
            }

            return Result.Fail<IEnumerable<(SegmentEffortModel, SegmentModel)>>(new GetAthleteKomsError(GetAthleteKomsError.UnknownError));
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

        protected Task DeactivateAthleteAsync(int athleteId)
        {
            return _komUoW.GetRepository<IAthleteRepository>()
                .DeactivateAthleteAsync(athleteId);
        }
    }
}
