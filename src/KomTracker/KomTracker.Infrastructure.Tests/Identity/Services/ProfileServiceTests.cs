using FluentAssertions;
using IdentityModel;
using IdentityServer4.Models;
using KomTracker.Infrastructure.Identity.Entities;
using KomTracker.Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Utils.Tests.UserManager;
using Xunit;

namespace KomTracker.Infrastructure.Tests.Identity.Services;

public class ProfileServiceTests
{
    private readonly UserManager<UserEntity> _userManager = TestUserManagerHelper.CreateTestUserManager<UserEntity>();
    private readonly ProfileService _profileService;
    private readonly UserEntity _user = new() { Id = "u1", AthleteId = 1 };

    public ProfileServiceTests()
    {
        _userManager.FindByIdAsync(_user.Id).Returns(_user);
        _userManager.UpdateAsync(_user).Returns(IdentityResult.Success);

        _profileService = new ProfileService(_userManager, Substitute.For<IUserClaimsPrincipalFactory<UserEntity>>(), Substitute.For<ILogger<ProfileService>>());
    }

    private static IsActiveContext Context(string subjectId)
        => new(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(JwtClaimTypes.Subject, subjectId) })), new Client(), "TokenEndpoint");

    [Fact]
    public async Task A_token_for_a_user_not_seen_for_a_while_records_last_seen()
    {
        _user.LastSeenAt = DateTime.UtcNow.AddDays(-3);
        var context = Context(_user.Id);

        await _profileService.IsActiveAsync(context);

        context.IsActive.Should().BeTrue();
        _user.LastSeenAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        await _userManager.Received(1).UpdateAsync(_user);
    }

    [Fact]
    public async Task A_user_seen_within_the_hour_is_not_written_again()
    {
        var recently = DateTime.UtcNow.AddMinutes(-10);
        _user.LastSeenAt = recently;

        await _profileService.IsActiveAsync(Context(_user.Id));

        _user.LastSeenAt.Should().Be(recently);
        await _userManager.DidNotReceive().UpdateAsync(Arg.Any<UserEntity>());
    }

    [Fact]
    public async Task A_failed_write_does_not_fail_the_token_request()
    {
        _userManager.UpdateAsync(_user).Returns<IdentityResult>(_ => throw new InvalidOperationException("db down"));

        var context = Context(_user.Id);
        await _profileService.IsActiveAsync(context);

        context.IsActive.Should().BeTrue();
    }
}
