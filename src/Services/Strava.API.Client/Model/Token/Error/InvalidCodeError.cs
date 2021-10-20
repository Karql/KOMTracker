using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strava.API.Client.Model.Token.Error
{
    public class InvalidCodeError : FluentResults.Error
    {
        public InvalidCodeError()
            : base("Invalid code!")
        {
        }
    }
}
