using AutoMapper;
using FluentAssertions;
using FluentResults;
using FluentResults.Extensions.FluentAssertions;
using KOMTracker.API.DAL;
using KOMTracker.API.DAL.Repositories;
using KOMTracker.API.Infrastructure.Services;
using KOMTracker.API.Models.Account.Error;
using KOMTracker.API.Models.Athlete;
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

namespace KOMTracker.API.Tests.Infrastructure.Services
{
    public class AthleteServiceTests
    {
        #region TestData
        private int TestAthleteId = 1;

        private TokenModel TestValidToken = new TokenModel
        {
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        };

        private TokenModel TestInvalidToken = new TokenModel
        {
            ExpiresAt = DateTime.UtcNow.AddMinutes(-60)
        };

        private TokenModel TestNewToken = new TokenModel
        {
            ExpiresAt = DateTime.UtcNow.AddHours(6)
        };

        #endregion

        private readonly IKOMUnitOfWork _komUoW;
        private readonly ITokenService _tokenService;
        private readonly IAthleteRepository _athleteRepository;

        private readonly AthleteService _athleteService;

        public AthleteServiceTests()
        {
            _athleteRepository = Substitute.For<IAthleteRepository>();
            _komUoW = new TestKOMUnitOfWork(new Dictionary<Type, IRepository>
            {
                { typeof(IAthleteRepository), _athleteRepository }
            });

            _tokenService = Substitute.For<ITokenService>();

            _athleteService = new AthleteService(_komUoW, _tokenService);
        }

        #region Get valid token
        [Fact]
        public async Task Get_valid_token_throw_when_no_token_in_db()
        {
            // Arrange
            _athleteRepository.GetTokenAsync(TestAthleteId).Returns((TokenModel)null);

            // Act
            Func<Task<Result<TokenModel>>> action = () => _athleteService.GetValidToken(TestAthleteId);

            // Assert
            await action.Should().ThrowAsync<Exception>();

            await _tokenService.DidNotReceiveWithAnyArgs().RefreshAsync(null);
            await _athleteRepository.DidNotReceiveWithAnyArgs().AddOrUpdateTokenAsync(null);
        }

        [Fact]
        public async Task Get_valid_token_return_token_directly_from_db_when_valid()
        {
            // Arrange
            _athleteRepository.GetTokenAsync(TestAthleteId).Returns(TestValidToken);

            // Act
            var res = await _athleteService.GetValidToken(TestAthleteId);

            // Assert
            res.Should().BeSuccess();
            res.Value.Should().Be(TestValidToken);

            await _tokenService.DidNotReceiveWithAnyArgs().RefreshAsync(null);
            await _athleteRepository.DidNotReceiveWithAnyArgs().AddOrUpdateTokenAsync(null);
        }

        [Fact]
        public async Task Get_valid_token_try_refresh_when_invalid()
        {
            // Arrange
            _athleteRepository.GetTokenAsync(TestAthleteId).Returns(TestInvalidToken);
            _tokenService.RefreshAsync(TestInvalidToken).Returns(Result.Ok(TestNewToken));

            // Act
            var res = await _athleteService.GetValidToken(TestAthleteId);

            // Assert
            res.Should().BeSuccess();
            res.Value.Should().Be(TestNewToken);

            await _athleteRepository.Received().AddOrUpdateTokenAsync(TestNewToken);
        }

        [Fact]
        public async Task Get_valid_token_throw_when_refresh_fail()
        {
            // Arrange
            _athleteRepository.GetTokenAsync(TestAthleteId).Returns(TestInvalidToken);
            _tokenService.RefreshAsync(TestInvalidToken).Returns(Result.Fail<TokenModel>(new RefreshError(RefreshError.InvalidRefreshToken)));

            // Act
            Func<Task<Result<TokenModel>>> action = () => _athleteService.GetValidToken(TestAthleteId);

            // Assert
            await action.Should().ThrowAsync<Exception>();

            await _athleteRepository.DidNotReceiveWithAnyArgs().AddOrUpdateTokenAsync(null);
        }

        #endregion
    }
}
