using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.AspNetCore.Helpers;

namespace Utils.AspNetCore.Extensions;

public static class IServiceProviderExtensions
{
    /// <summary>
    /// Based on eShopOnContainers.
    /// </summary>
    public static IServiceProvider MigrateDbContext<TContext>(this IServiceProvider services, Action<TContext, IServiceProvider> seeder = null) where TContext : DbContext
    {
        DbContextHelper.MigrateDbContext<TContext>(services, seeder);
        return services;
    }
}
