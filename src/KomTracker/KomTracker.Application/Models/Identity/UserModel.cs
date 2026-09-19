using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Application.Models.Identity;
public class UserModel
{
    /// <summary>Platform user id (AspNetUsers.Id) — the athlete→user bridge.</summary>
    public string? Id { get; set; }
    public int AthleteId { get; set; }
    public string? Email { get; set; }
    public bool EmailConfirmed { get; set; }
}
