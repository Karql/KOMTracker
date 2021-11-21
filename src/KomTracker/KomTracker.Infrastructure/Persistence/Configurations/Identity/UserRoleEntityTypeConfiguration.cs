using KomTracker.Infrastructure.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KomTracker.Infrastructure.Persistence.Configurations.Identity;

public class UserRoleEntityTypeConfiguration
    : IEntityTypeConfiguration<UserRoleEntity>
{
    public void Configure(EntityTypeBuilder<UserRoleEntity> builder)
    {
        builder.ToTable("user_role");

        builder.Property(x => x.UserId)
            .HasColumnName("user_id");

        builder.Property(x => x.RoleId)
            .HasColumnName("role_id");
    }
}
