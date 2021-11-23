using NSubstitute.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Extensions;

namespace Utils.Tests.Extensions;

public static class NSubstituteExtensions
{
    public static IEnumerable<ICall> FilterByName(this IEnumerable<ICall> receivedCalls, string methodName)
    {
        return receivedCalls?.Where(x => x.GetMethodInfo().Name == methodName);
    }

    public static T GetArgument<T>(this ICall receivedCalls)
    {
        if (receivedCalls == null)
            return default;

        var argumentType = typeof(T);
        return (T)receivedCalls
            .GetArguments()
            .FirstOrDefault(x => IsArgumentMatchType(argumentType, x.GetType()));
    }

    private static bool IsArgumentMatchType(Type argType, Type receivedCallArgType)
    {
        var result = true;
        if (argType.IsInterface)
        {
            if (!receivedCallArgType.IsImplementing(argType))
                result = false;
        }
        else if (receivedCallArgType != argType)
        {
            result = false;
        }
        return result;
    }
}
