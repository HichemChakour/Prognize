using Microsoft.EntityFrameworkCore;
using Prognize.Api.Common.Tenancy;
using Prognize.Api.Data;

namespace Prognize.Tests.Support;

public sealed class TestDatabase : IAsyncDisposable
{
    public string Name { get; } = $"prognize_test_{Guid.NewGuid():N}";

    public string ConnectionString =>
        $"Host=localhost;Port=5432;Database={Name};Username=prognize;Password=prognize_dev_pwd";

    public AppDbContext CreateContext(Guid? tenantId)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new AppDbContext(options, new FixedTenantContext(tenantId));
    }

    public async Task MigrateAsync()
    {
        await using var db = CreateContext(null);
        await db.Database.MigrateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await using var db = CreateContext(null);
        await db.Database.EnsureDeletedAsync();
    }
}
