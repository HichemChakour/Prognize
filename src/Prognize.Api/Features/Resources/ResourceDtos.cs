using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Prognize.Api.Domain.Enums;

namespace Prognize.Api.Features.Resources;

public record ResourceDto(
    Guid Id,
    string Name,
    string Kind,
    int? Capacity,
    ResourceStatus Status,
    JsonElement? Attributes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record CreateResourceRequest(
    [Required, MaxLength(200)] string Name,
    [Required, MaxLength(50)] string Kind,
    [Range(1, int.MaxValue)] int? Capacity,
    JsonElement? Attributes);

public record UpdateResourceRequest(
    [Required, MaxLength(200)] string Name,
    [Required, MaxLength(50)] string Kind,
    [Range(1, int.MaxValue)] int? Capacity,
    ResourceStatus Status,
    JsonElement? Attributes);
