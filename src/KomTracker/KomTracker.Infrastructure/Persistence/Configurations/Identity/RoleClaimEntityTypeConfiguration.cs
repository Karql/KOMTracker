using KomTracker.Infrastructure.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KomTracker.Infrastructure.Persistence.Configurations.Identity;

public class RoleClaimEntityTypeConfiguration
    : IEntityTypeConfiguration<RoleClaimEntity>
{
    public void Configure(EntityTypeBuilder<RoleClaimEntity> builder)
    {
        builder.ToTable("role_claim");

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.RoleId)
            .HasColumnName("role_id");

        builder.Property(x => x.ClaimType)
            .HasColumnName("claim_type");

        builder.Property(x => x.ClaimValue)
            .HasColumnName("claim_value");
    }
}
