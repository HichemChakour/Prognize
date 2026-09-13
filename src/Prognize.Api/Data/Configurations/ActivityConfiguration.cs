using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prognize.Api.Domain;

namespace Prognize.Api.Data.Configurations;

public class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> builder)
    {
        builder.ToTable("activity", table =>
        {
            table.HasCheckConstraint("ck_activity_duration_minutes", "duration_minutes > 0");
            table.HasCheckConstraint("ck_activity_priority", "priority BETWEEN 1 AND 5");
        });

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.Priority)
            .HasDefaultValue(3);

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.OwnsMany(a => a.Requirements, req =>
        {
            req.ToJson();
            req.Property(r => r.Kind).HasMaxLength(50);
        });

        builder.Property(a => a.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(a => new { a.OrganizationId, a.Name }).IsUnique();
    }
}
