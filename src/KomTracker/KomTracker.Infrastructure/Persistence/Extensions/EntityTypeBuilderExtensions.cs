using KomTracker.Domain.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Infrastructure.Persistence.Extensions;

public static class EntityTypeBuilderExtensions
{
    public static void PrepareBaseColumns<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : BaseEntity
    {
        builder.Property(x => x.AuditCD)
            .HasColumnName("audit_cd");

        builder.Property(x => x.AuditMD)
            .HasColumnName("audit_md");
    }
}
