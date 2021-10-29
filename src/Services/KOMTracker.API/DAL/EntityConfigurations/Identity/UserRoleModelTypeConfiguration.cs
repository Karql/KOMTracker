using KOMTracker.API.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KOMTracker.API.DAL.EntityConfigurations.Identity
{
    public class UserRoleModelTypeConfiguration
        : IEntityTypeConfiguration<UserRoleModel>
    {
        public void Configure(EntityTypeBuilder<UserRoleModel> builder)
        {
            builder.ToTable("user_role");

            builder.Property(x => x.UserId)
                .HasColumnName("user_id");

            builder.Property(x => x.RoleId)
                .HasColumnName("role_id");
        }
    }
}
