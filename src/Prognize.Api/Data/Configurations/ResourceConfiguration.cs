using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prognize.Api.Domain;

namespace Prognize.Api.Data.Configurations;

public class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        builder.ToTable("resource");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.Kind)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.Attributes)
            .HasColumnType("jsonb");

        builder.Property(r => r.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.OwnsMany(r => r.Availability, w =>
        {
            w.ToJson();
            w.Property(x => x.Day).HasConversion<string>();
        });

        builder.HasIndex(r => new { r.OrganizationId, r.Kind, r.Name }).IsUnique();
    }
}
