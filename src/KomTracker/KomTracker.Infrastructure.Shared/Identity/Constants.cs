﻿using System;
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