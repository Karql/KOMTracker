using KOMTracker.API.DAL.EntityConfigurations;
using KOMTracker.API.DAL.EntityConfigurations.Athlete;
using KOMTracker.API.DAL.EntityConfigurations.Identity;
using KOMTracker.API.DAL.EntityConfigurations.Segment;
using KOMTracker.API.DAL.EntityConfigurations.Token;
using KOMTracker.API.Models.Athlete;
using KOMTracker.API.Models.Identity;
using KOMTracker.API.Models.Segment;
using KOMTracker.API.Models.Token;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace KOMTracker.API.DAL;

public class KOMDBContext : IdentityDbContext<UserModel, RoleModel, string, UserClaimModel, UserRoleModel, UserLoginModel, RoleClaimModel, UserTokenModel>
{
#if DEBUG
    public static readonly ILoggerFactory DebugLoggerFactory = LoggerFactory.Create(builder =>
    {
        builder.AddDebug();
        builder.AddConsole();
    });


#endif

    private readonly ILoggerFactory _loggerFactory;

    public virtual DbSet<AthleteModel> Athlete { get; set; }

    public virtual DbSet<TokenModel> Token { get; set; }

    public virtual DbSet<SegmentModel> Segment { get; set; }

    public virtual DbSet<SegmentEffortModel> SegmentEffort { get; set; }

    public virtual DbSet<KomsSummaryModel> KomsSummary { get; set; }

    public KOMDBContext(DbContextOptions<KOMDBContext> options, ILoggerFactory loggerFactory)
        : base(options)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

#if DEBUG
        optionsBuilder.UseLoggerFactory(_loggerFactory);
        optionsBuilder.EnableDetailedErrors();
        optionsBuilder.EnableSensitiveDataLogging();
#endif
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Identity
        builder.ApplyConfiguration(new UserModelTypeConfiguration());
        builder.ApplyConfiguration(new RoleModelTypeConfiguration());
        builder.ApplyConfiguration(new UserClaimModelTypeConfiguration());
        builder.ApplyConfiguration(new UserRoleModelTypeConfiguration());
        builder.ApplyConfiguration(new UserLoginModelTypeConfiguration());
        builder.ApplyConfiguration(new RoleClaimModelTypeConfiguration());
        builder.ApplyConfiguration(new UserTokenModelTypeConfiguration());

        // Strava
        builder.ApplyConfiguration(new AthleteModelTypeConfiguration());
        builder.ApplyConfiguration(new TokenModelTypeConfiguration());
        builder.ApplyConfiguration(new SegmentModelTypeConfiguration());
        builder.ApplyConfiguration(new SegmentEffortModelTypeConfiguration());
        builder.ApplyConfiguration(new KomsSummaryModelTypeConfiguration());
        builder.ApplyConfiguration(new KomsSummarySegmentEffortModelTypeConfiguration());
    }
}
