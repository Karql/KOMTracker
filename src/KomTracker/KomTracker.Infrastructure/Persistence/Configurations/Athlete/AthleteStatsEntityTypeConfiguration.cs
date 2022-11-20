using KomTracker.Domain.Entities.Athlete;
using KomTracker.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KomTracker.Infrastructure.Persistence.Configurations.Athlete;

public class AthleteStatsEntityTypeConfiguration
    : IEntityTypeConfiguration<AthleteStatsEntity>
{
    public void Configure(EntityTypeBuilder<AthleteStatsEntity> builder)
    {
        builder.ToTable("athlete_stats");

        builder.PrepareBaseColumns();

        builder.HasOne(x => x.Athlete)
            .WithOne(x => x.Stats)
            .HasForeignKey<AthleteStatsEntity>(x => x.AthleteId);

        builder.HasKey(x => x.AthleteId);

        builder.Property(x => x.AthleteId)
            .HasColumnName("athlete_id")
            .ValueGeneratedNever(); // id from strava

        builder.Property(x => x.StatsJson)
            .HasColumnName("stats_json")
            .HasColumnType("json")
            .IsRequired();
    }
}
