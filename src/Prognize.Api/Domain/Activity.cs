using Prognize.Api.Common.Tenancy;
using Prognize.Api.Domain.Enums;

namespace Prognize.Api.Domain;

public class Activity : ITenantOwned
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public required string Name { get; set; }

    public int DurationMinutes { get; set; }

    public int Priority { get; set; } = 3;

    public List<ResourceRequirement> Requirements { get; set; } = [];

    public ActivityStatus Status { get; set; } = ActivityStatus.Active;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
