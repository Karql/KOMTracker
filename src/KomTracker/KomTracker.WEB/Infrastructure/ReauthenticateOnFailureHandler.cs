using System.Net;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace KomTracker.WEB.Infrastructure;

/// <summary>
/// Recovers from auth failures on API calls instead of crashing the app: when the access token
/// can't be renewed (e.g. the refresh token was invalidated by an API restart) or the API returns
/// 401, it sends the user through login again. Because the IdP session cookie survives, this
/// re-login is silent (no Strava prompt) and lands the user back where they were.
///
/// Must be registered as the OUTERMOST handler: <see cref="AuthorizationMessageHandler"/> throws
/// <see cref="AccessTokenNotAvailableException"/> before delegating, so only a wrapping handler can
/// catch it; it also observes the 401 bubbling back.
/// </summary>
public class ReauthenticateOnFailureHandler : DelegatingHandler
{
    private const string LoginPath = "authentication/login";

    private readonly NavigationManager _navigation;

    public ReauthenticateOnFailureHandler(NavigationManager navigation)
    {
        _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _navigation.NavigateToLogin(LoginPath);
            }

            return response;
        }
        catch (AccessTokenNotAvailableException)
        {
            _navigation.NavigateToLogin(LoginPath);
            throw;
        }
    }
}
