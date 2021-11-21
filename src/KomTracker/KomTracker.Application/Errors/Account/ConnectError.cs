using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KomTracker.Application.Errors.Account;

public class ConnectError : FluentResults.Error
{
    public const string NoRequiredScope = "No required scope!";
    public const string InvalidCode = "Invalid code";

    public ConnectError(string message)
        : base(message)
    {
    }
}
