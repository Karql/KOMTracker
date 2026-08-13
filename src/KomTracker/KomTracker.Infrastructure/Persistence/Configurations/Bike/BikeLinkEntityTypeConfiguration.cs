using KomTracker.Domain.Entities.Bike;
using KomTracker.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KomTracker.Infrastructure.Persistence.Configurations.Bike;

public class BikeLinkEntityTypeConfiguration : IEntityTypeConfiguration<BikeLinkEntity>
{
    public void Configure(EntityTypeBuilder<BikeLinkEntity> builder)
    {
        builder.ToTable("bike_link", "bt");

        builder.PrepareBaseColumns();

        builder.HasOne<BikeEntity>().WithMany().HasForeignKey(x => x.BikeId);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id"); // DB-generated identity

        builder.Property(x => x.BikeId).HasColumnName("bike_id");

        builder.Property(x => x.ExternalService)
            .HasColumnName("external_service")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired(true);

        builder.Property(x => x.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(50)
            .IsRequired(true);

        builder.HasIndex(x => x.BikeId);
        builder.HasIndex(x => new { x.ExternalService, x.ExternalId }).IsUnique();
    }
}
