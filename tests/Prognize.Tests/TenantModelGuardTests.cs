using Prognize.Api.Common.Tenancy;
using Prognize.Tests.Support;

namespace Prognize.Tests;

public class TenantModelGuardTests
{
    [Fact]
    public void Toute_entite_ITenantOwned_a_un_query_filter()
    {
        using var db = new TestDatabase().CreateContext(null);

        var unfiltered = db.Model.GetEntityTypes()
            .Where(t => typeof(ITenantOwned).IsAssignableFrom(t.ClrType))
            .Where(t => t.GetDeclaredQueryFilters().Count == 0)
            .Select(t => t.ClrType.Name)
            .ToList();

        Assert.True(unfiltered.Count == 0,
            $"Entités multi-tenant SANS query filter (fuite de données possible) : {string.Join(", ", unfiltered)}");
    }
}
