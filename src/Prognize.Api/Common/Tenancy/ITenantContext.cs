namespace Prognize.Api.Common.Tenancy;

public interface ITenantContext
{
    Guid? OrganizationId { get; }
}
