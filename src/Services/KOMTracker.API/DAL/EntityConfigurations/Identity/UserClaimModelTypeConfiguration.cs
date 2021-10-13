using KOMTracker.API.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KOMTracker.API.DAL.EntityConfigurations.Identity
{
    public class UserClaimModelTypeConfiguration
        : IEntityTypeConfiguration<UserClaimModel>
    {
        public void Configure(EntityTypeBuilder<UserClaimModel> builder)
        {
            builder.ToTable("identity_user_claim");

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.UserId)
                .HasColumnName("user_id");

            builder.Property(x => x.ClaimType)
                .HasColumnName("claim_type");

            builder.Property(x => x.ClaimValue)
                .HasColumnName("claim_value");
        }
    }
}
