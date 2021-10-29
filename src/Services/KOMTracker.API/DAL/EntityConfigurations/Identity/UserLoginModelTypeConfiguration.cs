using KOMTracker.API.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KOMTracker.API.DAL.EntityConfigurations.Identity
{
    public class UserLoginModelTypeConfiguration
        : IEntityTypeConfiguration<UserLoginModel>
    {
        public void Configure(EntityTypeBuilder<UserLoginModel> builder)
        {
            builder.ToTable("user_login");

            builder.Property(x => x.UserId)
                .HasColumnName("user_id");

            builder.Property(x => x.LoginProvider)
                .HasColumnName("login_provider");

            builder.Property(x => x.ProviderKey)
                .HasColumnName("provider_key");

            builder.Property(x => x.ProviderDisplayName)
                .HasColumnName("provider_display_name");
        }
    }
}
