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
        #region TestData
        private const string TEST_INVALID_CODE = "invalid";
        private const string TEST_EXISTING_ATHLETE_CODE = "exist";
        private const string TEST_NEW_ATHLETE_CODE = "new";
        private const string TEST_INVALID_SCOPE = "read";
        private const string TEST_VALID_SCOPE = "read,activity:read,profile:read_all";

        private AthleteModel TestExistingAthlete = new AthleteModel
        {
            AthleteId = 1,
            Username = "Athlete1"
        };
        private AthleteModel TestNewAthlete = new AthleteModel
        {
            AthleteId = 2,
            Username = "Athlete2"

        };
        private TokenModel TestToken = new TokenModel();
        #endregion

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

            PrepareMocks();
        }

        #region Connect
        [Fact]
        public async Task Connect_check_is_scope_contains_required()
        {
            // Act
            var res = await _accountService.Connect(TEST_INVALID_CODE, TEST_INVALID_SCOPE);

            // Assert
            res.Should().BeFailure();
            res.HasError<ConnectError>(x => x.Message == ConnectError.NoRequiredScope).Should().BeTrue();

            await AssertConnectNoUpdate();
        }

        [Fact]
        public async Task Connect_failed_when_invalid_code()
        {
            // Act
            var res = await _accountService.Connect(TEST_INVALID_CODE, TEST_VALID_SCOPE);

            // Assert
            res.Should().BeFailure();
            res.HasError<ConnectError>(x => x.Message == ConnectError.InvalidCode).Should().BeTrue();

            await AssertConnectNoUpdate();
        }

        [Fact]
        public async Task Connect_add_new_athlete()
        {
            // Act
            var res = await _accountService.Connect(TEST_NEW_ATHLETE_CODE, TEST_VALID_SCOPE);

            // Assert
            await _athleteService.Received().AddOrUpdateAthleteAsync(TestNewAthlete);
            await _athleteService.Received().AddOrUpdateTokenAsync(TestToken);
            await _userManager.Received().CreateAsync(Arg.Is<UserModel>(x =>
                x.AthleteId == TestNewAthlete.AthleteId
                && x.UserName == TestNewAthlete.Username));
            await _komUoW.Received().SaveChangesAsync();

            res.Should().BeSuccess();
        }

        [Fact]
        public async Task Connect_update_exsisting_athlete()
        {
            // Act
            var res = await _accountService.Connect(TEST_EXISTING_ATHLETE_CODE, TEST_VALID_SCOPE);

            // Assert
            await _athleteService.Received().AddOrUpdateAthleteAsync(TestExistingAthlete);
            await _athleteService.Received().AddOrUpdateTokenAsync(TestToken);
            await _userManager.DidNotReceiveWithAnyArgs().CreateAsync(null);
            await _komUoW.Received().SaveChangesAsync();

            res.Should().BeSuccess();
        }

        private void PrepareMocks()
        {
            var users = new List<UserModel> { 
                new UserModel { 
                    AthleteId = TestExistingAthlete.AthleteId, 
                    UserName = TestExistingAthlete.Username
                }
            }
            .AsQueryable()
            .BuildMock();
            _userManager.Users.Returns(users);

            _tokenService.ExchangeAsync(TEST_INVALID_CODE, TEST_VALID_SCOPE).Returns(Result.Fail(new ExchangeError(ExchangeError.InvalidCode)));

            _tokenService.ExchangeAsync(TEST_INVALID_CODE, TEST_VALID_SCOPE).Returns(Result.Fail(new ExchangeError(ExchangeError.InvalidCode)));
            
            _tokenService.ExchangeAsync(TEST_EXISTING_ATHLETE_CODE, TEST_VALID_SCOPE).Returns(Result.Ok((TestExistingAthlete, TestToken)));

            _tokenService.ExchangeAsync(TEST_NEW_ATHLETE_CODE, TEST_VALID_SCOPE).Returns(Result.Ok((TestNewAthlete, TestToken)));
        }

        private async Task AssertConnectNoUpdate()
        {
            await _athleteService.DidNotReceiveWithAnyArgs().AddOrUpdateAthleteAsync(null);
            await _athleteService.DidNotReceiveWithAnyArgs().AddOrUpdateTokenAsync(null);
            await _userManager.DidNotReceiveWithAnyArgs().CreateAsync(null);
            await _komUoW.DidNotReceiveWithAnyArgs().SaveChangesAsync();
        }
        #endregion
    }
}
