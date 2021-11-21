using KomTracker.Infrastructure.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KomTracker.Infrastructure.Persistence.Configurations.Identity;

public class UserEntityTypeConfiguration
    : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("user");

        builder.HasOne(x => x.Athlete)
            .WithOne()
            .HasForeignKey<UserEntity>(x => x.AthleteId);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.UserName)
            .HasColumnName("username");

        builder.Property(x => x.NormalizedUserName)
            .HasColumnName("normalized_username");

        builder.Property(x => x.Email)
            .HasColumnName("email");

        builder.Property(x => x.NormalizedEmail)
            .HasColumnName("normalized_email");

        builder.Property(x => x.Email)
            .HasColumnName("email");

        builder.Property(x => x.EmailConfirmed)
            .HasColumnName("email_confirmed");

        builder.Property(x => x.PasswordHash)
            .HasColumnName("password_hash");

        builder.Property(x => x.SecurityStamp)
            .HasColumnName("security_stamp");

        builder.Property(x => x.ConcurrencyStamp)
            .HasColumnName("concurrency_stamp");

        builder.Property(x => x.PhoneNumber)
            .HasColumnName("phone_number");

        builder.Property(x => x.PhoneNumberConfirmed)
            .HasColumnName("phone_number_confirmed");

        builder.Property(x => x.TwoFactorEnabled)
            .HasColumnName("two_factor_enabled");

        builder.Property(x => x.LockoutEnd)
            .HasColumnName("lockout_end");

        builder.Property(x => x.LockoutEnabled)
            .HasColumnName("lockout_enabled");

        builder.Property(x => x.AccessFailedCount)
            .HasColumnName("access_failed_count");

        builder.Property(x => x.AthleteId)
            .HasColumnName("athlete_id");
    }
}
