using KomTracker.Domain.Entities.Token;
using KomTracker.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KomTracker.Infrastructure.Persistence.Configurations.Token;

public class TokenEntityTypeConfiguration
    : IEntityTypeConfiguration<TokenEntity>
{
    public void Configure(EntityTypeBuilder<TokenEntity> builder)
    {
        builder.ToTable("token");

        builder.PrepareBaseColumns();

        builder.HasOne(x => x.Athlete)
            .WithOne(x => x.Token)
            .HasForeignKey<TokenEntity>(x => x.AthleteId);

        builder.HasKey(x => x.AthleteId);

        builder.Property(x => x.AthleteId)
            .HasColumnName("athlete_id")
            .ValueGeneratedNever(); // id from strava

        builder.Property(x => x.TokenType)
            .HasColumnName("token_type")
            .HasMaxLength(10);

        builder.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at");

        builder.Property(x => x.AccessToken)
            .HasColumnName("access-token")
            .HasMaxLength(50);

        builder.Property(x => x.RefreshToken)
            .HasColumnName("refresh-token")
            .HasMaxLength(50);

        builder.Property(x => x.Scope)
            .HasColumnName("scope")
            .HasMaxLength(100);
    }
}
