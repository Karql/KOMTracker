using AutoMapper;
using FluentAssertions;
using FluentResults;
using FluentResults.Extensions.FluentAssertions;
using KOMTracker.API.DAL;
using KOMTracker.API.Infrastructure.Services;
using KOMTracker.API.Models.Account.Error;
using KOMTracker.API.Models.Athlete;
using KOMTracker.API.Models.Identity;
using KOMTracker.API.Models.Token;
using KOMTracker.API.Models.Token.Error;
using Microsoft.AspNetCore.Identity;
using MockQueryable.NSubstitute;
using NSubstitute;
using Strava.API.Client.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using ApiModel = Strava.API.Client.Model;

namespace KOMTracker.API.Tests.Infrastructure.Services
{
    public class AccountServiceTests
    {
        private const string TEST_CODE = "xxx";
        private const string TEST_INVALID_SCOPE = "read";
        private const string TEST_VALID_SCOPE = "read,activity:read,profile:read_all";

        private readonly IMapper _mapper;
        private readonly IKOMUnitOfWork _komUoW;
        private readonly ITokenService _tokenService;
        private readonly IAthleteService _athleteService;
        private readonly IUserStore<UserModel> _userStore;
        private readonly UserManager<UserModel> _userManager;

        private readonly AccountService _accountService;

        public AccountServiceTests()
        {
            _mapper = Substitute.For<IMapper>();
            _komUoW = Substitute.For<IKOMUnitOfWork>();
            _tokenService = Substitute.For<ITokenService>();
            _athleteService = Substitute.For<IAthleteService>();

            _userStore = Substitute.For<IUserStore<UserModel>>();
            _userManager = Substitute.For<UserManager<UserModel>>(_userStore, null, null, null, null, null, null, null, null);

            _accountService = new AccountService(_mapper, _komUoW, _tokenService, _athleteService, _userManager);
        }

        #region Connect
        [Fact]
        public async Task Connect_check_is_scope_contains_required()
        {
            // Act
            var res = await _accountService.Connect(TEST_CODE, TEST_INVALID_SCOPE);

            // Assert
            res.Should().BeFailure();
            res.HasError<ConnectError>(x => x.Message == ConnectError.NoRequiredScope).Should().BeTrue();
        }

        [Fact]
        public async Task Connect_failed_when_invalid_code()
        {
            // Arrange
            _tokenService.ExchangeAsync(TEST_CODE, TEST_VALID_SCOPE).Returns(Result.Fail(new ExchangeError(ExchangeError.InvalidCode)));

            // Act
            var res = await _accountService.Connect(TEST_CODE, TEST_VALID_SCOPE);

            // Assert
            res.Should().BeFailure();
            res.HasError<ConnectError>(x => x.Message == ConnectError.InvalidCode).Should().BeTrue();
        }

        [Fact]
        public async Task Connect_add_or_update_athlete_and_token()
        {
            // Arrange
            var users = new List<UserModel>()
                .AsQueryable()
                .BuildMock();
            var athlete = new AthleteModel();
            var token = new TokenModel();

            _userManager.Users.Returns(users);
            _tokenService.ExchangeAsync(TEST_CODE, TEST_VALID_SCOPE).Returns(Result.Ok((athlete, token)));

            // Act
            var res = await _accountService.Connect(TEST_CODE, TEST_VALID_SCOPE);

            // Assert
            await _athleteService.Received().AddOrUpdateAthleteAsync(athlete);
            await _athleteService.Received().AddOrUpdateTokenAsync(token);
        }
        #endregion
    }
}
