using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KomTracker.Infrastructure.Entities.Identity;

public class UserLoginEntity : IdentityUserLogin<string>
{
}
