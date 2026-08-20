using KomTracker.Domain.Entities.Warehouse;
using KomTracker.Infrastructure.Identity.Entities;
using KomTracker.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KomTracker.Infrastructure.Persistence.Configurations.Warehouse;

public class WarehouseEntityTypeConfiguration
    : IEntityTypeConfiguration<WarehouseEntity>
{
    public void Configure(EntityTypeBuilder<WarehouseEntity> builder)
    {
        builder.ToTable("warehouse", "bt");

        builder.PrepareBaseColumns();

        // Owner (scoping). Strong FK to the identity user, referenceless (no navigation).
        builder.HasOne<UserEntity>().WithMany().HasForeignKey(x => x.UserId);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id"); // DB-generated identity

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired(true);

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired(true);

        builder.HasIndex(x => x.UserId);
    }
}
