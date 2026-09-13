using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Prognize.Api.Domain.Enums;

namespace Prognize.Api.Features.Schedules;

public record ConflictDto(ConflictType Type, string Message, Guid? ResourceId);

public record AssignedResourceDto(Guid Id, string Name, string Kind, int? Capacity);

public record AssignmentDto(
    Guid Id,
    Guid ScheduleId,
    Guid ActivityId,
    string ActivityName,
    int Priority,
    DateTime Start,
    DateTime End,
    IReadOnlyList<AssignedResourceDto> Resources,
    IReadOnlyList<ConflictDto> Conflicts);

public record ScheduleSummaryDto(
    Guid Id,
    string Name,
    ScheduleStatus Status,
    double? Score,
    int AssignmentCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record ScheduleDetailDto(
    Guid Id,
    string Name,
    ScheduleStatus Status,
    double? Score,
    JsonElement? Metrics,
    IReadOnlyList<AssignmentDto> Assignments,
    int ConflictCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record CreateScheduleRequest([Required, MaxLength(200)] string Name);

public record UpdateScheduleRequest([Required, MaxLength(200)] string Name, ScheduleStatus Status);

public record UpsertAssignmentRequest(
    [Required] Guid ActivityId,
    [Required] DateTime Start,
    [Required] DateTime End,
    [Required] IReadOnlyList<Guid> ResourceIds);
