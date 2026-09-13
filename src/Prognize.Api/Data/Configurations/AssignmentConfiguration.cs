using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prognize.Api.Domain;

namespace Prognize.Api.Data.Configurations;

public class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> builder)
    {
        builder.ToTable("assignment", table =>
        {
            table.HasCheckConstraint("ck_assignment_period", "\"end\" > \"start\"");
        });

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Start).HasColumnType("timestamp without time zone");
        builder.Property(a => a.End).HasColumnType("timestamp without time zone");

        builder.HasOne(a => a.Activity)
            .WithMany()
            .HasForeignKey(a => a.ActivityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Resources)
            .WithOne(ar => ar.Assignment)
            .HasForeignKey(ar => ar.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(a => a.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(a => new { a.ScheduleId, a.Start });
    }
}
