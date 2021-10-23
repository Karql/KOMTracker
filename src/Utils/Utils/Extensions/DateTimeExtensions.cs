using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Extensions
{
    public static class DateTimeExtensions
    {
        public static long ToTimeStamp(this DateTime value)
        {
            return Convert.ToInt64((value - DateTime.UnixEpoch).TotalSeconds);
        }

        public static string ToUtcIso(this DateTime value)
        {
            return value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
        }
    }
}
