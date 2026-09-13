using Prognize.Api.Common.Tenancy;

namespace Prognize.Api.Domain;

public class AssignmentResource : ITenantOwned
{
    public Guid AssignmentId { get; set; }

    public Assignment? Assignment { get; set; }

    public Guid ResourceId { get; set; }

    public Resource? Resource { get; set; }

    public Guid OrganizationId { get; set; }
}
