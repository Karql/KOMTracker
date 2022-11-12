using KomTracker.Domain.Entities.Athlete;
using KomTracker.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KomTracker.Infrastructure.Persistence.Configurations.Athlete;

public class AthleteClubEntityTypeConfiguration
    : IEntityTypeConfiguration<AthleteClubEntity>
{
    public void Configure(EntityTypeBuilder<AthleteClubEntity> builder)
    {
        builder.ToTable("athlete_club");

        builder.PrepareBaseColumns();

        builder.Property(x => x.AthleteId)
            .HasColumnName("athlete_id");

        builder.Property(x => x.ClubId)
            .HasColumnName("club_id");
    }
}
