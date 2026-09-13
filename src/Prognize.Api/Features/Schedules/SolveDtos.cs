using System.ComponentModel.DataAnnotations;

namespace Prognize.Api.Features.Schedules;

public record SolveRequest(
    [Required] DateOnly WeekStart,
    IReadOnlyList<DayOfWeek>? Days = null,
    TimeOnly? DayStart = null,
    TimeOnly? DayEnd = null,
    [Range(1, 120)] int TimeLimitSeconds = 10);
