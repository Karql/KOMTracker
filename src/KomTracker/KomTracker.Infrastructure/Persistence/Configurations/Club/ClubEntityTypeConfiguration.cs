using KomTracker.Domain.Entities.Club;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using KomTracker.Infrastructure.Persistence.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Infrastructure.Persistence.Configurations.Club;

public class ClubEntityTypeConfiguration
    : IEntityTypeConfiguration<ClubEntity>
{
    public void Configure(EntityTypeBuilder<ClubEntity> builder)
    {
        builder.ToTable("club");

        builder.PrepareBaseColumns();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(255);

        builder.Property(x => x.ProfileMedium)
            .HasColumnName("profile_medium")
            .HasMaxLength(255);

        builder.Property(x => x.Profile)
            .HasColumnName("profile")
            .HasMaxLength(255);

        builder.Property(x => x.CoverPhoto)
            .HasColumnName("cover_photo")
            .HasMaxLength(255);

        builder.Property(x => x.CoverPhotoSmall)
            .HasColumnName("cover_photo_small")
            .HasMaxLength(255);

        builder.Property(x => x.ActivityTypesIcon)
            .HasColumnName("activity_types_icon")
            .HasMaxLength(100);

        builder.Property(x => x.SportType)
            .HasColumnName("sport_type")
            .HasMaxLength(100);

        builder.Property(x => x.City)
            .HasColumnName("city")
            .HasMaxLength(255);

        builder.Property(x => x.State)
            .HasColumnName("state")
            .HasMaxLength(255);

        builder.Property(x => x.Country)
            .HasColumnName("country")
            .HasMaxLength(255);

        builder.Property(x => x.Private)
            .HasColumnName("private");

        builder.Property(x => x.MemberCount)
            .HasColumnName("member_count");

        builder.Property(x => x.Featured)
            .HasColumnName("featured");

        builder.Property(x => x.Verified)
            .HasColumnName("verified");

        builder.Property(x => x.Url)
            .HasColumnName("url")
            .HasMaxLength(255);
    }
}