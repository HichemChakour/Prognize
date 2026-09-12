using System.Text.Json;
using Prognize.Api.Common.Tenancy;
using Prognize.Api.Domain.Enums;

namespace Prognize.Api.Domain;

public class Resource : ITenantOwned
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public required string Name { get; set; }

    public required string Kind { get; set; }

    public int? Capacity { get; set; }

    public ResourceStatus Status { get; set; } = ResourceStatus.Active;

    public JsonElement? Attributes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
