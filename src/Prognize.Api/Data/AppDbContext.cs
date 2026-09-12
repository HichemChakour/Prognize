using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Prognize.Api.Common.Tenancy;
using Prognize.Api.Domain;

namespace Prognize.Api.Data;

public class AppDbContext : IdentityUserContext<AppUser, Guid>
{
    private readonly ITenantContext _tenant;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenant)
        : base(options)
    {
        _tenant = tenant;
    }

    public DbSet<Organization> Organizations => Set<Organization>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        builder.Entity<IdentityUserClaim<Guid>>().ToTable("app_user_claim");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("app_user_login");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("app_user_token");

        builder.Entity<Organization>()
            .HasQueryFilter(o => o.Id == _tenant.OrganizationId);

        builder.Entity<AppUser>()
            .HasQueryFilter(u => u.OrganizationId == _tenant.OrganizationId);
    }

    public override int SaveChanges()
    {
        ApplyTenantRules();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplyTenantRules();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyTenantRules()
    {
        var currentTenant = _tenant.OrganizationId;

        if (currentTenant is null)
            return;

        foreach (EntityEntry<ITenantOwned> entry in ChangeTracker.Entries<ITenantOwned>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.OrganizationId = currentTenant.Value;
                    break;
                case EntityState.Modified:
                    entry.Property(nameof(ITenantOwned.OrganizationId)).IsModified = false;
                    break;
            }
        }
    }
}
