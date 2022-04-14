﻿using KomTracker.Application.Models.Identity;
using KomTracker.Domain.Entities.Athlete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Application.Interfaces.Services.Identity;

public interface IUserService
{
    Task<bool> IsUserExistsAsync(int athleteId);
    Task<UserModel?> GetUserAsync(int athleteId);
    Task AddUserAsync(AthleteEntity athlete);

    Task ChangeUserMailAsync(int athleteId, string newEmail);
}
