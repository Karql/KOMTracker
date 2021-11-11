using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using Utils.AspNetCore.Extensions;
using Utils.AspNetCore.Helpers;

namespace Microsoft.AspNetCore.Hosting;

/// <summary>
/// Based on eShopOnContainers.
/// </summary>
public static class IHostExtensions
{
    public static IHost MigrateDbContext<TContext>(this IHost host, Action<TContext, IServiceProvider> seeder = null) where TContext : DbContext
    {
        DbContextHelper.MigrateDbContext(host.Services, seeder);
        return host;
    }
}
