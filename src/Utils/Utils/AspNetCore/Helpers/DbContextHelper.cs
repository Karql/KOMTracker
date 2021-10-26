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

namespace Utils.AspNetCore.Helpers
{
    public static class DbContextHelper
    {
        /// <summary>
        /// Based on eShopOnContainers.
        /// </summary>
        public static void MigrateDbContext<TContext>(IServiceProvider serviceProvider, Action<TContext, IServiceProvider> seeder = null) where TContext : DbContext
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<TContext>>();
                var context = services.GetRequiredService<TContext>();

                try
                {
                    logger.LogInformation("Migrating database associated with context {DbContextName}", typeof(TContext).Name);

                    var retries = 10;
                    var retry = Policy
                        .Handle<DbException>()
                        .WaitAndRetry(
                            retryCount: retries,
                            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                            onRetry: (exception, timeSpan, retry, ctx) =>
                            {
                                logger.LogWarning(exception, "[{prefix}] Exception {ExceptionType} with message {Message} detected on attempt {retry} of {retries}", nameof(TContext), exception.GetType().Name, exception.Message, retry, retries);
                            });

                    // if the sql server container is not created on run docker compose this
                    // migration can't fail for network related exception. The retry options for DbContext only 
                    // apply to transient exceptions
                    // Note that this is NOT applied when running some orchestrators (let the orchestrator to recreate the failing service)
                    retry.Execute(() => context.Database.Migrate());
                    if (seeder != null)
                    {
                        retry.Execute(() => seeder(context, services));
                    }

                    logger.LogInformation("Migrated database associated with context {DbContextName}", typeof(TContext).Name);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while migrating the database used on context {DbContextName}", typeof(TContext).Name);
                    throw; // rethrow service should not run when migration failed
                }
            }
        }
    }
}
