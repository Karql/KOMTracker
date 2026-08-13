using KomTracker.Domain.Entities.Athlete;
using KomTracker.Domain.Entities.Strava;
using KomTracker.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KomTracker.Infrastructure.Persistence.Configurations.Strava;

public class StravaBikeEntityTypeConfiguration : IEntityTypeConfiguration<StravaBikeEntity>
{
    public void Configure(EntityTypeBuilder<StravaBikeEntity> builder)
    {
        builder.ToTable("bike", "strava");

        builder.PrepareBaseColumns();

        builder.HasOne<AthleteEntity>().WithMany().HasForeignKey(x => x.AthleteId);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasMaxLength(50).ValueGeneratedNever(); // gear id from strava

        builder.Property(x => x.AthleteId).HasColumnName("athlete_id");

        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(255);
        builder.Property(x => x.Nickname).HasColumnName("nickname").HasMaxLength(255);
        builder.Property(x => x.Primary).HasColumnName("primary");
        builder.Property(x => x.Retired).HasColumnName("retired");

        builder.Property(x => x.Distance).HasColumnName("distance");
        builder.Property(x => x.ConvertedDistance).HasColumnName("converted_distance");

        builder.Property(x => x.BrandName).HasColumnName("brand_name").HasMaxLength(255);
        builder.Property(x => x.ModelName).HasColumnName("model_name").HasMaxLength(255);
        builder.Property(x => x.FrameType).HasColumnName("frame_type");
        builder.Property(x => x.Description).HasColumnName("description");
        builder.Property(x => x.Weight).HasColumnName("weight");

        builder.HasIndex(x => x.AthleteId);
    }
}
