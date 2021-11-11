using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KOMTracker.API.Models.Identity;

public class UserClaimModel : IdentityUserClaim<string>
{
}
