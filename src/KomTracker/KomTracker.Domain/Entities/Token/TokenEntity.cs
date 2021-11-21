using KomTracker.Domain.Entities.Athlete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KomTracker.Domain.Entities.Token;

public class TokenEntity
{
    public int AthleteId { get; set; }
    public string TokenType { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public string Scope { get; set; }
    public virtual AthleteEntity Athlete { get; set; }
}
