using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Extensions;

public static class BoolExtensions
{
    public static string ToLowerString(this bool value)
    {
        return value ? "true" : "false";
    }
}
