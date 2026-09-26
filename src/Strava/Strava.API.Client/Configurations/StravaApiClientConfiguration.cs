using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strava.API.Client.Configurations;

public class StravaApiClientConfiguration
{
    public int ClientID { get; set; }
    public string ClientSecret { get; set; }

    /// <summary>Token echoed back during the webhook subscription GET validation handshake (D-12).</summary>
    public string WebhookVerifyToken { get; set; }

    /// <summary>Private secret embedded in the webhook callback path (strava/callback/{secret}) — our only gate on the unsigned callbacks.</summary>
    public string WebhookSecret { get; set; }
}
