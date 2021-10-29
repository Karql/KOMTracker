using KOMTracker.API.Models.Token;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KOMTracker.API.DAL.EntityConfigurations.Strava
{
    public class TokenModelTypeConfiguration
        : IEntityTypeConfiguration<TokenModel>
    {
        public void Configure(EntityTypeBuilder<TokenModel> builder)
        {
            builder.ToTable("token");

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

            builder.HasOne(x => x.Athlete)
                .WithOne(x => x.Token)
                .HasForeignKey<TokenModel>(x => x.AthleteId);
        }
    }
}
