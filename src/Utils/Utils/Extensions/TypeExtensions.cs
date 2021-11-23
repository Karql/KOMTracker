using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Extensions;

public static class TypeExtensions
{
    public static bool IsImplementing<T>(this Type type)
    {
        return type.IsImplementing(typeof(T));
    }

    public static bool IsImplementing(this Type type, Type implementInterface)
    {
        var fullName = type.FullName;

        if (implementInterface.IsGenericType)
        {
            return type.GetInterfaces().Any(t => t.IsGenericType
                && t.GetGenericTypeDefinition() == implementInterface.GetGenericTypeDefinition()
                && t.GetGenericArguments().SequenceEqual(implementInterface.GetGenericArguments()));
        }

        return type.GetInterfaces().Contains(implementInterface);
    }
}