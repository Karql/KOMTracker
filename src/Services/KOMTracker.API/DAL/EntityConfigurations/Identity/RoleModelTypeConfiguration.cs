using KOMTracker.API.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KOMTracker.API.DAL.EntityConfigurations.Identity;

public class RoleModelTypeConfiguration
    : IEntityTypeConfiguration<RoleModel>
{
    public void Configure(EntityTypeBuilder<RoleModel> builder)
    {
        builder.ToTable("role");

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.Name)
            .HasColumnName("name");

        builder.Property(x => x.NormalizedName)
            .HasColumnName("normalized_name");

        builder.Property(x => x.ConcurrencyStamp)
            .HasColumnName("concurrency_stamp");
    }
}
