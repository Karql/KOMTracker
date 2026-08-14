using System;
using System.Text;
using FluentAssertions;
using KomTracker.Infrastructure.Identity.Endpoints;
using Xunit;

namespace KomTracker.Infrastructure.Tests.Identity;

public class StravaAuthorizeUrlTests
{
    [Fact]
    public void Build_emits_scopes_prompt_redirect_and_base64_state()
    {
        var url = StravaAuthorizeUrl.Build(
            clientId: 123,
            scopes: new[] { "read", "activity:read_all", "profile:read_all" },
            approvalPrompt: "force",
            redirectUri: "https://host/identity/account/connect-upgrade",
            returnUrl: "https://web/account?tab=strava");

        var expectedState = Convert.ToBase64String(Encoding.UTF8.GetBytes("https://web/account?tab=strava"));

        url.Should().StartWith("https://www.strava.com/oauth/authorize?");
        url.Should().Contain("approval_prompt=force");
        url.Should().Contain("scope=read,activity:read_all,profile:read_all");
        url.Should().Contain("client_id=123");
        url.Should().Contain("response_type=code");
        url.Should().Contain("redirect_uri=https://host/identity/account/connect-upgrade");
        url.Should().Contain($"state={expectedState}");
    }
}
