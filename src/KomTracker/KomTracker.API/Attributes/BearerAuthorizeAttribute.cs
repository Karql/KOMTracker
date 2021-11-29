using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace KomTracker.API.Attributes;

/// <summary>
/// TODO:
/// IdentityServer adds Cookie Authentication
/// I have not found way to limit this cookie only for /identity paths
/// https://stackoverflow.com/a/57045827/11391667
/// https://github.com/aspnet/Security/issues/1479#issuecomment-360928524
/// To not lose time I have made this attribute to allow only Bearer
/// </summary>
public class BearerAuthorizeAttribute : AuthorizeAttribute
{
    public BearerAuthorizeAttribute()
    {
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme;
    }
}
