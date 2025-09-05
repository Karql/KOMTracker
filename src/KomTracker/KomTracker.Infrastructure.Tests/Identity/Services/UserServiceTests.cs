using AutoMapper;
using FluentAssertions;
using KomTracker.Domain.Entities.Athlete;
using KomTracker.Infrastructure.Identity.Configurations;
using KomTracker.Infrastructure.Identity.Entities;
using KomTracker.Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utils.Tests.UserManager;
using Xunit;

namespace KomTracker.Infrastructure.Tests.Identity.Services;

public class UserServiceTests
{
    #region TestData
    private AthleteEntity TestExistingAthlete = new AthleteEntity
    {
        AthleteId = 1,
        Username = "Athlete1"
    };

    private AthleteEntity TestNewAthlete = new AthleteEntity
    {
        AthleteId = 2,
        Username = "Athlete2"

    };
    #endregion

    private readonly IMapper _mapper;
    private readonly UserManager<UserEntity> _userManager;
    private readonly UserService _userService;
    private readonly IdentityConfiguration _identityConfiguration;

    public UserServiceTests()
    {
        _mapper = Substitute.For<IMapper>();
        _userManager = TestUserManagerHelper.CreateTestUserManager<UserEntity>();
        _identityConfiguration = Substitute.For<IdentityConfiguration>();

        _userService = new UserService(_mapper, _userManager, _identityConfiguration);

        PrepareMocks();
    }

    #region IsUserExistsAsync
    [Fact]
    public async Task Is_user_exists_returns_true_when_exists()
    {
        // Act
        var res = await _userService.IsUserExistsAsync(TestExistingAthlete.AthleteId);

        // Assert
        res.Should().Be(true);
    }

    [Fact]
    public async Task Is_user_exists_returns_false_when_not_exists()
    {
        // Act
        var res = await _userService.IsUserExistsAsync(TestNewAthlete.AthleteId);

        // Assert
        res.Should().Be(false);
    }
    #endregion

    #region AddUserAsync
    [Fact]
    public async Task Add_user_calls_UserManager()
    {
        // Act
        await _userService.AddUserAsync(TestNewAthlete);

        // Assert
        await _userManager.Received().CreateAsync(Arg.Is<UserEntity>(x =>
            x.AthleteId == TestNewAthlete.AthleteId
            && x.UserName == TestNewAthlete.Username));
    }
    #endregion

    private void PrepareMocks()
    {
        var users = new List<UserEntity> {
            new UserEntity {
                AthleteId = TestExistingAthlete.AthleteId,
                UserName = TestExistingAthlete.Username
            }
        };

        TestUserManagerHelper.MockUsers(_userManager, users);
    }
}
