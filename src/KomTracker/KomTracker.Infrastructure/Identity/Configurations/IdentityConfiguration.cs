using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Infrastructure.Identity.Configurations;

public class IdentityConfiguration
{
    public string IdentityUrl { get; set; }
    public string Authority { get; set; }
    public bool RequireHttpsMetadata { get; set; } = true;
    public List<string> RedirectUris { get; set; }
    public List<string> PostLogoutRedirectUris { get; set; }
}
