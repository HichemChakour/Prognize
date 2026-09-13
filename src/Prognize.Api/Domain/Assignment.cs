using Prognize.Api.Common.Tenancy;

namespace Prognize.Api.Domain;

public class Assignment : ITenantOwned
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid ScheduleId { get; set; }

    public Schedule? Schedule { get; set; }

    public Guid ActivityId { get; set; }

    public Activity? Activity { get; set; }

    public DateTime Start { get; set; }

    public DateTime End { get; set; }

    public List<AssignmentResource> Resources { get; set; } = [];

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
