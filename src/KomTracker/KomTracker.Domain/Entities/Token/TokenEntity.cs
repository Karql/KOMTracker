﻿using KomTracker.Domain.Contracts;
using KomTracker.Domain.Entities.Athlete;
using System;
using System.Text.Json.Serialization;

namespace KomTracker.Domain.Entities.Token;

public class TokenEntity : BaseEntity
{
    public int AthleteId { get; set; }
    public string TokenType { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public string Scope { get; set; }
    [JsonIgnore]
    public virtual AthleteEntity Athlete { get; set; }
}
