namespace Prognize.Api.Common.Tenancy;

public interface ITenantOwned
{
    Guid OrganizationId { get; set; }
}
