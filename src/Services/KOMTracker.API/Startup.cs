using KOMTracker.API.DAL;
using KOMTracker.API.DAL.Repositories;
using KOMTracker.API.Infrastructure.Services;
using KOMTracker.API.Models.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Strava.API.Client.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KOMTracker.API
{
    public class Startup
    {
        private static readonly string AppName = "KOM Tracker";

        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = AppName,
                    Version = "1.0.0"
                });
            });

            services.AddDbContext<KOMDBContext>(options => options.UseNpgsql(_configuration.GetConnectionString("DB")));

            services.AddScoped<IKOMUnitOfWork, EFKOMUnitOfWork>();
            services.AddScoped<IAthleteRepository, EFAthleteRepository>();

            services.AddIdentity<UserModel, IdentityRole>()
                .AddEntityFrameworkStores<KOMDBContext>()
                .AddDefaultTokenProviders();

            services.AddTransient<IAthleteService, AthleteService>();
            services.AddStravaApiClient();
            services.AddTransient<Strava.API.Client.Model.Config.ConfigModel>(sp =>
            {
                return _configuration.GetSection("StravaApiClientConfig").Get<Strava.API.Client.Model.Config.ConfigModel>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"v1/swagger.json", AppName);
            });

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
