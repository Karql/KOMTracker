using KomTracker.Domain.Entities.ServiceRecord;
using KomTracker.Infrastructure.Identity.Entities;
using KomTracker.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KomTracker.Infrastructure.Persistence.Configurations.ServiceRecord;

public class ServiceRecordEntityTypeConfiguration
    : IEntityTypeConfiguration<ServiceRecordEntity>
{
    public void Configure(EntityTypeBuilder<ServiceRecordEntity> builder)
    {
        builder.ToTable("service", "bt");

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

        builder.Property(x => x.Date)
            .HasColumnName("date")
            .IsRequired(true);

        builder.Property(x => x.Place)
            .HasColumnName("place")
            .HasMaxLength(200);

        builder.Property(x => x.Cost)
            .HasColumnName("cost");

        builder.Property(x => x.Notes)
            .HasColumnName("notes")
            .HasMaxLength(2000);

        // Deleting a service deletes what it covered (the targets are part of the record).
        builder.HasMany(x => x.Targets)
            .WithOne(x => x.Service)
            .HasForeignKey(x => x.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId);
    }
}
