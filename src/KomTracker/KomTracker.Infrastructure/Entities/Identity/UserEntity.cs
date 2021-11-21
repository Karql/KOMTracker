using KomTracker.Domain.Entities.Athlete;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KomTracker.Infrastructure.Entities.Identity;

public class UserEntity : IdentityUser
{
    public int AthleteId { get; set; }
    public virtual AthleteEntity Athlete { get; set; }
}
