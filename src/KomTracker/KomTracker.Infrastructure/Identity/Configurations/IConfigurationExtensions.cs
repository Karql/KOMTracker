using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KomTracker.Infrastructure.Shared.Identity.Constants;

namespace KomTracker.Infrastructure.Identity.Configurations;

public static class IConfigurationExtensions
{
    public static IdentityConfiguration GetIdentityConfiguration(this IConfiguration configuration)
    {
        return configuration
            .GetSection(ConfigurationSections.IdentityConfiguration)
            .Get<IdentityConfiguration>();
    }
}
