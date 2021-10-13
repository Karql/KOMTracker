using KOMTracker.API.Models.Strava;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KOMTracker.API.DAL.EntityConfigurations.Strava
{
    public class AthleteModelTypeConfiguration
        : IEntityTypeConfiguration<AthleteModel>
    {
        public void Configure(EntityTypeBuilder<AthleteModel> builder)
        {
            builder.ToTable("strava_athlete");

            builder.HasKey(x => x.AthleteId);

            builder.Property(x => x.AthleteId)
                .HasColumnName("athlete_id")
                .ValueGeneratedNever(); // id from strava

            builder.Property(x => x.UserId)
                .HasColumnName("user_id");

            builder.Property(x => x.FirstName)
                .HasColumnName("first_name");

            builder.Property(x => x.LastName)
                .HasColumnName("last_name");

            builder.Property(x => x.Bio)
                .HasColumnName("bio");

            builder.Property(x => x.City)
                .HasColumnName("city");

            builder.Property(x => x.Country)
                .HasColumnName("country");

            builder.Property(x => x.Sex)
                .HasColumnName("sex");

            builder.Property(x => x.Weight)
                .HasColumnName("weight");

            builder.Property(x => x.Profile)
                .HasColumnName("profile");

            builder.Property(x => x.ProfileMedium)
                .HasColumnName("profile_medium");

            builder.HasOne(x => x.User)
                .WithOne(x => x.Athlete)
                .HasForeignKey<AthleteModel>(x => x.UserId);
        }
    }
}
