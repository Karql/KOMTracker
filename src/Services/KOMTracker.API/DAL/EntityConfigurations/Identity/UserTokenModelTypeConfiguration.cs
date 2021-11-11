using KOMTracker.API.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KOMTracker.API.DAL.EntityConfigurations.Identity;

public class UserTokenModelTypeConfiguration
    : IEntityTypeConfiguration<UserTokenModel>
{
    public void Configure(EntityTypeBuilder<UserTokenModel> builder)
    {
        builder.ToTable("user_token");

        builder.Property(x => x.UserId)
            .HasColumnName("user_id");

        builder.Property(x => x.LoginProvider)
            .HasColumnName("login_provider");

        builder.Property(x => x.Name)
            .HasColumnName("name");

        builder.Property(x => x.Value)
            .HasColumnName("value");
    }
}
