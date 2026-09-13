using System.Text.Json;
using Prognize.Api.Common.Tenancy;
using Prognize.Api.Domain.Enums;

namespace Prognize.Api.Domain;

public class Schedule : ITenantOwned
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public required string Name { get; set; }

    public ScheduleStatus Status { get; set; } = ScheduleStatus.Draft;

    public double? Score { get; set; }

    public JsonElement? Metrics { get; set; }

    public List<Assignment> Assignments { get; set; } = [];

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
