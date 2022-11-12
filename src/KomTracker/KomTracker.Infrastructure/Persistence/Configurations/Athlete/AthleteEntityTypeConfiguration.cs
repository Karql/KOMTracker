using KomTracker.Domain.Entities.Athlete;
using KomTracker.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KomTracker.Infrastructure.Persistence.Configurations.Athlete;

public class AthleteEntityTypeConfiguration
    : IEntityTypeConfiguration<AthleteEntity>
{
    public void Configure(EntityTypeBuilder<AthleteEntity> builder)
    {
        builder.ToTable("athlete");

        builder.PrepareBaseColumns();

        builder.HasMany(x => x.Clubs)
            .WithMany(x => x.Athletes)
            .UsingEntity<AthleteClubEntity>(
                x => x.HasOne(x => x.Club).WithMany().HasForeignKey(x => x.ClubId),
                x => x.HasOne(x => x.Athlete).WithMany().HasForeignKey(x => x.AthleteId)
            );

        builder.HasKey(x => x.AthleteId);

        builder.Property(x => x.AthleteId)
            .HasColumnName("athlete_id")
            .ValueGeneratedNever(); // id from strava

        builder.Property(x => x.Username)
            .HasColumnName("username")
            .HasMaxLength(100);

        builder.Property(x => x.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(100);

        builder.Property(x => x.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(100);

        builder.Property(x => x.Bio)
            .HasColumnName("bio")
            .HasMaxLength(1000);

        builder.Property(x => x.City)
            .HasColumnName("city")
            .HasMaxLength(100);

        builder.Property(x => x.Country)
            .HasColumnName("country")
            .HasMaxLength(100);

        builder.Property(x => x.Sex)
            .HasColumnName("sex")
            .HasMaxLength(10);

        builder.Property(x => x.Weight)
            .HasColumnName("weight");

        builder.Property(x => x.Profile)
            .HasColumnName("profile")
            .HasMaxLength(255);

        builder.Property(x => x.ProfileMedium)
            .HasColumnName("profile_medium")
            .HasMaxLength(255);
    }
}
