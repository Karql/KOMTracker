﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KOMTracker.API.Models.Token.Error;

public class RefreshError : FluentResults.Error
{
    public const string InvalidRefreshToken = "Invalid refresh token";
    public const string UnknownError = "Unknown error";

    public RefreshError(string message)
        : base(message)
    {
    }
}
