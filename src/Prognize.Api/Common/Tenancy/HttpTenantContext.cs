using System.Security.Claims;

namespace Prognize.Api.Common.Tenancy;

public class HttpTenantContext : ITenantContext
{

    public const string OrganizationClaimType = "org_id";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpTenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? OrganizationId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User
                ?.FindFirst(OrganizationClaimType)?.Value;

            return Guid.TryParse(claim, out var orgId) ? orgId : null;
        }
    }
}
public class FixedTenantContext : ITenantContext
{
    public FixedTenantContext(Guid? organizationId) => OrganizationId = organizationId;

    public Guid? OrganizationId { get; }
}
