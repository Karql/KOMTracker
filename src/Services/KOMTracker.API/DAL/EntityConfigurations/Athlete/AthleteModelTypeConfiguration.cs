using KOMTracker.API.Models.Athlete;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KOMTracker.API.DAL.EntityConfigurations.Athlete
{
    public class AthleteModelTypeConfiguration
        : IEntityTypeConfiguration<AthleteModel>
    {
        public void Configure(EntityTypeBuilder<AthleteModel> builder)
        {
            builder.ToTable("athlete");

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
}
