using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strava.API.Client.Model.Base.Error
{
    public abstract class BaseError : FluentResults.Error
    {
        public const string Unauthorized = "Unauthorized";

        public BaseError(string message)
            : base(message)
        {
        }
    }
}
