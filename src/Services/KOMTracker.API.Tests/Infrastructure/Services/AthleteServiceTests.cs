using AutoFixture;
using AutoMapper;
using FluentAssertions;
using FluentResults;
using FluentResults.Extensions.FluentAssertions;
using KOMTracker.API.DAL;
using KOMTracker.API.DAL.Repositories;
using KOMTracker.API.Infrastructure.Services;
using KOMTracker.API.Models.Account.Error;
using KOMTracker.API.Models.Athlete;
using KOMTracker.API.Models.Athlete.Error;
using KOMTracker.API.Models.Identity;
using KOMTracker.API.Models.Token;
using KOMTracker.API.Models.Token.Error;
using KOMTracker.API.Tests.DAL;
using Microsoft.AspNetCore.Identity;
using MockQueryable.NSubstitute;
using NSubstitute;
using Strava.API.Client.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Tests.UserManager;
using Utils.UnitOfWork.Abstract;
using Xunit;
using ApiModel = Strava.API.Client.Model;
using MoreLinq;
using KOMTracker.API.Models.Segment;

namespace KOMTracker.API.Tests.Infrastructure.Services
{
    public class AthleteServiceTests
    {
        #region TestData
        private int TestAthleteId = 1;

        private TokenModel TestValidToken => new TokenModel
        {
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        };

        private TokenModel TestInvalidToken => new TokenModel
        {
            ExpiresAt = DateTime.UtcNow.AddMinutes(-60)
        };

        private TokenModel TestNewToken => new TokenModel
        {
            ExpiresAt = DateTime.UtcNow.AddHours(6)
        };

        #endregion

        private readonly IMapper _mapper;
        private readonly IKOMUnitOfWork _komUoW;
        private readonly ITokenService _tokenService;
        private readonly IAthleteApi _athleteApi;
        private readonly IAthleteRepository _athleteRepository;

        private readonly AthleteService _athleteService;

        public AthleteServiceTests()
        {
            _athleteRepository = Substitute.For<IAthleteRepository>();
            _komUoW = new TestKOMUnitOfWork(new Dictionary<Type, IRepository>
            {
                { typeof(IAthleteRepository), _athleteRepository }
            });

            _mapper = Substitute.For<IMapper>();
            _tokenService = Substitute.For<ITokenService>();
            _athleteApi = Substitute.For<IAthleteApi>();

            _athleteService = new AthleteService(_mapper, _komUoW, _tokenService, _athleteApi);
        }

        #region Get valid token
        [Fact]
        public async Task Get_valid_return_error_and_decativate_athlete_when_no_token_in_db()
        {
            // Arrange
            _athleteRepository.GetTokenAsync(TestAthleteId).Returns((TokenModel)null);

            // Act
            var res = await _athleteService.GetValidTokenAsync(TestAthleteId);

            // Assert
            res.Should().BeFailure();
            res.HasError<GetValidTokenError>(x => x.Message == GetValidTokenError.NoTokenInDB).Should().BeTrue();

            await _athleteRepository.Received().DeactivateAthleteAsync(TestAthleteId);
            await _tokenService.DidNotReceiveWithAnyArgs().RefreshAsync(null);
            await _athleteRepository.DidNotReceiveWithAnyArgs().AddOrUpdateTokenAsync(null);
        }

        [Fact]
        public async Task Get_valid_token_return_token_directly_from_db_when_valid()
        {
            // Arrange
            var testValidToken = TestValidToken;
            _athleteRepository.GetTokenAsync(TestAthleteId).Returns(testValidToken);

            // Act
            var res = await _athleteService.GetValidTokenAsync(TestAthleteId);

            // Assert
            res.Should().BeSuccess();
            res.Value.Should().BeEquivalentTo(testValidToken);

            await _tokenService.DidNotReceiveWithAnyArgs().RefreshAsync(null);
            await _athleteRepository.DidNotReceiveWithAnyArgs().AddOrUpdateTokenAsync(null);
        }

        [Fact]
        public async Task Get_valid_token_try_refresh_when_invalid()
        {
            // Arrange
            var testInvalidToken = TestInvalidToken;
            var testNewToken = TestNewToken;
            _athleteRepository.GetTokenAsync(TestAthleteId).Returns(testInvalidToken);
            _tokenService.RefreshAsync(testInvalidToken).Returns(Result.Ok(testNewToken));

            // Act
            var res = await _athleteService.GetValidTokenAsync(TestAthleteId);

            // Assert
            res.Should().BeSuccess();
            res.Value.Should().BeEquivalentTo(testNewToken);

            await _athleteRepository.Received().AddOrUpdateTokenAsync(testNewToken);
        }

        [Fact]
        public async Task Get_valid_token_return_error_and_decativate_athlete_when_refresh_fail()
        {
            // Arrange
            var testInvalidToken = TestInvalidToken;
            _athleteRepository.GetTokenAsync(TestAthleteId).Returns(testInvalidToken);
            _tokenService.RefreshAsync(testInvalidToken).Returns(Result.Fail<TokenModel>(new RefreshError(RefreshError.InvalidRefreshToken)));

            // Act
            var res = await _athleteService.GetValidTokenAsync(TestAthleteId);

            // Assert
            res.Should().BeFailure();
            res.HasError<GetValidTokenError>(x => x.Message == GetValidTokenError.RefreshFailed).Should().BeTrue();

            await _athleteRepository.Received().DeactivateAthleteAsync(TestAthleteId);
            await _athleteRepository.DidNotReceiveWithAnyArgs().AddOrUpdateTokenAsync(null);
        }
        #endregion

        #region Get athlete koms
        [Fact]
        public async Task Get_athlete_koms_call_api_and_returns_only_public_koms()
        {
            // Arrange
            var effortsCount = 5;
            var privateCount = 2;
            var fixture = new Fixture();
            var apiKoms = fixture.CreateMany<ApiModel.Segment.SegmentEffortDetailedModel>(effortsCount);
            var expectedKoms = new List<(SegmentEffortModel, SegmentModel)>();
            apiKoms.ForEach((x, i) => {                
                if (!(x.Segment.Private = i < privateCount))
                {
                    var segmentEffort = fixture.Create<SegmentEffortModel>();
                    var segment = fixture.Create<SegmentModel>();
                    _mapper.Map<SegmentEffortModel>(x).Returns(segmentEffort);
                    _mapper.Map<SegmentModel>(x.Segment).Returns(segment);

                    expectedKoms.Add((segmentEffort, segment));
                }
            });

            var token = TestValidToken.AccessToken;
            _athleteApi.GetKomsAsync(TestAthleteId, token).Returns(Result.Ok(apiKoms.AsEnumerable()));

            // Act
            var res = await _athleteService.GetAthleteKomsAsync(TestAthleteId, token);

            // Assert
            res.Should().BeSuccess();
            var actualKoms = res.Value;

            actualKoms.Should().BeEquivalentTo(expectedKoms);
        }

        [Theory]
        [InlineData(ApiModel.Segment.Error.GetKomsError.Unauthorized, GetAthleteKomsError.Unauthorized)]
        [InlineData(ApiModel.Segment.Error.GetKomsError.UnknownError, GetAthleteKomsError.UnknownError)]
        public async Task Get_athlete_koms_pass_error(string apiError, string serviceError)
        {
            // Arrange
            var token = TestInvalidToken.AccessToken;
            _athleteApi.GetKomsAsync(TestAthleteId, token).Returns(Result.Fail<IEnumerable<ApiModel.Segment.SegmentEffortDetailedModel>>(new ApiModel.Segment.Error.GetKomsError(apiError)));

            // Act
            var res = await _athleteService.GetAthleteKomsAsync(TestAthleteId, token);

            // Assert
            res.Should().BeFailure();
            res.HasError<GetAthleteKomsError>(x => x.Message == serviceError).Should().BeTrue();
        }
        #endregion
    }
}
