using KomTracker.Infrastructure.Persistence.Configurations.Athlete;
using KomTracker.Infrastructure.Persistence.Configurations.Identity;
using KomTracker.Infrastructure.Persistence.Configurations.Segment;
using KomTracker.Infrastructure.Persistence.Configurations.Token;
using KomTracker.Domain.Entities.Athlete;
using KomTracker.Infrastructure.Entities.Identity;
using KomTracker.Domain.Entities.Segment;
using KomTracker.Domain.Entities.Token;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace KomTracker.Infrastructure.Persistence;

public class KOMDBContext : IdentityDbContext<UserEntity, RoleEntity, string, UserClaimEntity, UserRoleEntity, UserLoginEntity, RoleClaimEntity, UserTokenEntity>
{
#if DEBUG
    public static readonly ILoggerFactory DebugLoggerFactory = LoggerFactory.Create(builder =>
    {
        builder.AddDebug();
        builder.AddConsole();
    });


#endif

    private readonly ILoggerFactory _loggerFactory;

    public virtual DbSet<AthleteEntity> Athlete { get; set; }

    public virtual DbSet<TokenEntity> Token { get; set; }

    public virtual DbSet<SegmentEntity> Segment { get; set; }

    public virtual DbSet<SegmentEffortEntity> SegmentEffort { get; set; }

    public virtual DbSet<KomsSummaryEntity> KomsSummary { get; set; }

    public virtual DbSet<KomsSummarySegmentEffortEntity> KomsSummarySegmentEffort { get; set; }

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
        builder.ApplyConfiguration(new UserEntityTypeConfiguration());
        builder.ApplyConfiguration(new RoleEntityTypeConfiguration());
        builder.ApplyConfiguration(new UserClaimEntityTypeConfiguration());
        builder.ApplyConfiguration(new UserRoleEntityTypeConfiguration());
        builder.ApplyConfiguration(new UserLoginEntityTypeConfiguration());
        builder.ApplyConfiguration(new RoleClaimEntityTypeConfiguration());
        builder.ApplyConfiguration(new UserTokenEntityTypeConfiguration());

        // Strava
        builder.ApplyConfiguration(new AthleteEntityTypeConfiguration());
        builder.ApplyConfiguration(new TokenEntityTypeConfiguration());
        builder.ApplyConfiguration(new SegmentEntityTypeConfiguration());
        builder.ApplyConfiguration(new SegmentEffortEntityTypeConfiguration());
        builder.ApplyConfiguration(new KomsSummaryEntityTypeConfiguration());
        builder.ApplyConfiguration(new KomsSummarySegmentEffortEntityTypeConfiguration());
    }
}
