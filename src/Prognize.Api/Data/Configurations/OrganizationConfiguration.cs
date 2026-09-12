using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prognize.Api.Domain;

namespace Prognize.Api.Data.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        
        builder.ToTable("organization");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Name)
            .HasMaxLength(200)      
            .IsRequired();          

        builder.Property(o => o.Slug)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(o => o.Slug).IsUnique();

        builder.Property(o => o.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasMany(o => o.Users)
            .WithOne(u => u.Organization)
            .HasForeignKey(u => u.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
