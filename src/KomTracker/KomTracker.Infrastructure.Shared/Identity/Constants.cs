using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Infrastructure.Shared.Identity;

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
        public const string ConfirmEmailChange = "ConfirmEmailChange";
    }

    public static class ProtocolRoutePaths
    {
        public const string Loing = "/account/login";
        public const string Connect = "/account/connect";
        public const string ConfirmEmailChange = "/account/confirm-email-change";
    }

    public static class ConfigurationSections
    {
        public const string IdentityConfiguration = "IdentityConfiguration";
    }

    public static class Claims
    {
        public const string AthleteId = "athlete_id";
        public const string FirstName = "first_name";
        public const string LastName = "last_name";
        public const string Username = "username";
        public const string Avatar = "avatar";
        public const string Email = "email";
        public const string EmailVerified = "email_verified";
        public const string Password = "password";
    }

    public static class OAuth2
    {
        public const string ClientId = "kom-tracker";
        public const string ClientName = "KOM Tracker";
        public const string CodeFlow = "code";
        public static class Scopes
        {
            public const string OpenId = "openid";
            public const string Profile = "profile";
            public const string OfflineAccess = "offline_access";
            public const string Api = "api";
        }
    }
}