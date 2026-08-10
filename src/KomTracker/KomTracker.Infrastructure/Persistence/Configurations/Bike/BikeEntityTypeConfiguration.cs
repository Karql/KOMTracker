using KomTracker.Domain.Entities.Bike;
using KomTracker.Infrastructure.Identity.Entities;
using KomTracker.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KomTracker.Infrastructure.Persistence.Configurations.Bike;

public class BikeEntityTypeConfiguration
    : IEntityTypeConfiguration<BikeEntity>
{
    public void Configure(EntityTypeBuilder<BikeEntity> builder)
    {
        builder.ToTable("bike", "bt");

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

        builder.Property(x => x.Brand)
            .HasColumnName("brand")
            .HasMaxLength(200);

        builder.Property(x => x.Model)
            .HasColumnName("model")
            .HasMaxLength(200);

        // Enum stored by name (string) — self-documenting, order-independent.
        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired(true);

        builder.Property(x => x.WeightKg)
            .HasColumnName("weight_kg");

        builder.Property(x => x.Notes)
            .HasColumnName("notes")
            .HasMaxLength(2000);

        builder.Property(x => x.Price)
            .HasColumnName("price");

        builder.Property(x => x.PurchasePlace)
            .HasColumnName("purchase_place")
            .HasMaxLength(200);

        builder.Property(x => x.PurchaseDate)
            .HasColumnName("purchase_date");

        builder.Property(x => x.InitialDistanceKm)
            .HasColumnName("initial_distance_km");

        builder.Property(x => x.InitialMovingHours)
            .HasColumnName("initial_moving_hours");

        builder.Property(x => x.InitialElevationM)
            .HasColumnName("initial_elevation_m");

        builder.Property(x => x.Lifecycle)
            .HasColumnName("lifecycle")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired(true);

        builder.Property(x => x.SaleDate)
            .HasColumnName("sale_date");

        builder.Property(x => x.SalePrice)
            .HasColumnName("sale_price");

        builder.HasIndex(x => x.UserId);
    }
}
