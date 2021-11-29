using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Infrastructure.Identity;

public static class Constants
{
    public static class IdentityServer
    {
        public static string Area = "/identity";
    }

    public static class EndpointNames
    {
        public const string Login = "Login";
        public const string Connect = "Connect";
    }

    public static class ProtocolRoutePaths
    {
        public const string Loing = "/account/login";
        public const string Connect = "/account/connect";
    }

    public static class ConfigurationSections
    {
        public const string IdentityConfiguration = "IdentityConfiguration";
    }

    public static class Claims
    {
        public const string AthleteId = "athlete_id";
    }

    public static class OAuth2
    {
        public const string ClientId = "kom-tracker";
        public const string ScopeApi = "api";
    }
}