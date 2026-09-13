using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prognize.Api.Domain;

namespace Prognize.Api.Data.Configurations;

public class AssignmentResourceConfiguration : IEntityTypeConfiguration<AssignmentResource>
{
    public void Configure(EntityTypeBuilder<AssignmentResource> builder)
    {
        builder.ToTable("assignment_resource");

        builder.HasKey(ar => new { ar.AssignmentId, ar.ResourceId });

        builder.HasOne(ar => ar.Resource)
            .WithMany()
            .HasForeignKey(ar => ar.ResourceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ar => ar.ResourceId);
    }
}
