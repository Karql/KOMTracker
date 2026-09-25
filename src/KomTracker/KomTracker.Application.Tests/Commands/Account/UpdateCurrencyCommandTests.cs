#nullable enable
using FluentAssertions;
using FluentResults;
using FluentResults.Extensions.FluentAssertions;
using KomTracker.Application.Commands.Account;
using NSubstitute;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using IIdentityUserService = KomTracker.Application.Interfaces.Services.Identity.IUserService;

namespace KomTracker.Application.Tests.Commands.Account;

public class UpdateCurrencyCommandTests
{
    private readonly IIdentityUserService _userService = Substitute.For<IIdentityUserService>();

    [Fact]
    public async Task Handler_delegates_to_user_service()
    {
        _userService.UpdateCurrencyAsync(7, "EUR").Returns(Result.Ok());
        var handler = new UpdateCurrencyCommandHandler(_userService);

        var res = await handler.Handle(new UpdateCurrencyCommand { AthleteId = 7, Currency = "EUR" }, CancellationToken.None);

        res.Should().BeSuccess();
        await _userService.Received().UpdateCurrencyAsync(7, "EUR");
    }

    [Theory]
    [InlineData("PLN", true)]
    [InlineData("EUR", true)]
    [InlineData("CHF", true)]
    [InlineData("usd", false)]   // codes are upper-case
    [InlineData("XXX", false)]
    [InlineData("", false)]
    public void Validator_accepts_only_supported_codes(string currency, bool expectedValid)
    {
        var result = new UpdateCurrencyCommandValidator()
            .Validate(new UpdateCurrencyCommand { AthleteId = 1, Currency = currency });

        result.IsValid.Should().Be(expectedValid);
    }
}
