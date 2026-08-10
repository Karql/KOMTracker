using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.API.Shared.Models.User;
public class UserModel
{
    /// <summary>Identity user id (JWT `sub`). Owner key for platform-agnostic data (e.g. BikeTracker).</summary>
    public string? UserId { get; set; }
    public int AthleteId { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string Avatar { get; set; } = default!;
    public string? Email { get; set; }
}

