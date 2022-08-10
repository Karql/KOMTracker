using Microsoft.Extensions.DependencyInjection;
using Strava.API.Client.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strava.API.Client;

public static class DependencyInjection
{
    public static void AddStravaApiClient(this IServiceCollection services)
    {
        services.AddHttpClient();

        services.AddTransient<ITokenApi, TokenApi>();
        services.AddTransient<IAthleteApi, AthleteApi>();
        services.AddTransient<ISegmentApi, SegmentApi>();
        services.AddTransient<IClubApi, ClubApi>();
    }
}
