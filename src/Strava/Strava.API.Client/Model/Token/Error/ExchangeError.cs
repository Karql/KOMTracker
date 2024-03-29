﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strava.API.Client.Model.Token.Error;

public class ExchangeError : FluentResults.Error
{
    public const string InvalidCode = "Invalid code";
    public const string UnknownError = "Unknown error";

    public ExchangeError(string message)
        : base(message)
    {
    }
}
