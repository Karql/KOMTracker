using System;
using System.Collections.Generic;
using System.Text;

namespace KomTracker.Infrastructure.Identity.Endpoints;

/// <summary>
/// Builds the Strava OAuth authorize URL. Shared by the login (`approval_prompt=auto`, base scopes)
/// and the scope-upgrade (`approval_prompt=force`, wider scopes) flows. The app's returnUrl is
/// base64-encoded into `state` and echoed back by Strava to the redirect (connect) endpoint.
/// </summary>
public static class StravaAuthorizeUrl
{
    public static string Build(int clientId, IEnumerable<string> scopes, string approvalPrompt, string redirectUri, string returnUrl)
    {
        var scope = string.Join(",", scopes);
        var state = Convert.ToBase64String(Encoding.UTF8.GetBytes(returnUrl));

        return $"https://www.strava.com/oauth/authorize?approval_prompt={approvalPrompt}&scope={scope}&client_id={clientId}&response_type=code&redirect_uri={redirectUri}&state={state}";
    }
}
