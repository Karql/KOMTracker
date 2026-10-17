using KomTracker.Domain.Entities.Expense;
using KomTracker.Infrastructure.Identity.Entities;
using KomTracker.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KomTracker.Infrastructure.Persistence.Configurations.Expense;

public class ExpenseEntityTypeConfiguration
    : IEntityTypeConfiguration<ExpenseEntity>
{
    public void Configure(EntityTypeBuilder<ExpenseEntity> builder)
    {
        builder.ToTable("expense", "bt");

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

        // Enum persisted by name (D-13).
        builder.Property(x => x.Category)
            .HasColumnName("category")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired(true);

        builder.Property(x => x.Price)
            .HasColumnName("price")
            .IsRequired(true);

        builder.Property(x => x.PurchaseDate)
            .HasColumnName("purchase_date")
            .IsRequired(true);

        builder.Property(x => x.PurchasePlace)
            .HasColumnName("purchase_place")
            .HasMaxLength(200);

        builder.Property(x => x.Notes)
            .HasColumnName("notes")
            .HasMaxLength(2000);

        builder.Property(x => x.Lifecycle)
            .HasColumnName("lifecycle")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired(true);

        builder.Property(x => x.SaleDate)
            .HasColumnName("sale_date");

        builder.Property(x => x.SalePrice)
            .HasColumnName("sale_price");

        builder.Property(x => x.SalePlace)
            .HasColumnName("sale_place")
            .HasMaxLength(200);

        builder.HasIndex(x => x.UserId);
    }
}
