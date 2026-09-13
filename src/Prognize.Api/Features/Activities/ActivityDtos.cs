using System.ComponentModel.DataAnnotations;
using Prognize.Api.Domain.Enums;

namespace Prognize.Api.Features.Activities;

public record ResourceRequirementDto(
    [Required, MaxLength(50)] string Kind,
    Guid? ResourceId,
    [Range(1, int.MaxValue)] int? MinCapacity);

public record ActivityDto(
    Guid Id,
    string Name,
    int DurationMinutes,
    int Priority,
    IReadOnlyList<ResourceRequirementDto> Requirements,
    ActivityStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record CreateActivityRequest(
    [Required, MaxLength(200)] string Name,
    [Range(1, 1440)] int DurationMinutes,
    [Range(1, 5)] int Priority,
    [Required] IReadOnlyList<ResourceRequirementDto> Requirements);

public record UpdateActivityRequest(
    [Required, MaxLength(200)] string Name,
    [Range(1, 1440)] int DurationMinutes,
    [Range(1, 5)] int Priority,
    [Required] IReadOnlyList<ResourceRequirementDto> Requirements,
    ActivityStatus Status);
