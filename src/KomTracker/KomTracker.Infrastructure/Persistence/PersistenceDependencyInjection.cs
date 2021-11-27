using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Infrastructure.Persistence;

public static class PersistenceDependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IKOMUnitOfWork, EFKOMUnitOfWork>();
        services.AddScoped<IAthleteRepository, EFAthleteRepository>();
        services.AddScoped<ISegmentRepository, EFSegmentRepository>();
        services.AddDbContext<KOMDBContext>(options => options.UseNpgsql(configuration.GetConnectionString("DB")));

        return services;
    }
}
