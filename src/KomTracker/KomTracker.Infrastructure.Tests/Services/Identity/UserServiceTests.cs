using FluentAssertions;
using KomTracker.Domain.Entities.Athlete;
using KomTracker.Infrastructure.Entities.Identity;
using KomTracker.Infrastructure.Services.Identity;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Tests.UserManager;

namespace KomTracker.Infrastructure.Tests.Services.Identity;

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

    private readonly UserManager<UserEntity> _userManager;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userManager = TestUserManagerHelper.CreateTestUserManager<UserEntity>();

        _userService = new UserService(_userManager);

        PrepareMocks();
    }

    #region IsUserExistsAsync
    public async Task Is_user_exists_return_true_when_exists()
    {
        // Act
        var res = await _userService.IsUserExistsAsync(TestExistingAthlete.AthleteId);

        // Assert
        res.Should().Be(true);
    }

    public async Task Is_user_exists_return_false_when_not_exists()
    {
        // Act
        var res = await _userService.IsUserExistsAsync(TestNewAthlete.AthleteId);

        // Assert
        res.Should().Be(false);
    }
    #endregion

    #region AddUserAsync
    public async Task Add_user_call_UserManager()
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
    }
}
