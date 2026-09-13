using Prognize.Api.Domain;
using Prognize.Solver.Model;

namespace Prognize.Api.Features.Schedules;

/// <summary>
/// Traduit les entités EF en ProblemModel pour le solveur, et la solution en entités.
/// C'est le SEUL endroit où l'API et le solveur se parlent : le solveur ne connaît
/// ni EF, ni Resource, ni Activity — uniquement ses propres records.
/// </summary>
public static class SolverBridge
{
    public static ProblemModel ToProblem(
        SolveRequest request, IReadOnlyList<Resource> resources, IReadOnlyList<Activity> activities)
    {
        var horizon = new SolverHorizon(
            request.WeekStart,
            request.Days is { Count: > 0 } ? request.Days : [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday],
            request.DayStart ?? new TimeOnly(8, 0),
            request.DayEnd ?? new TimeOnly(18, 0));

        var solverResources = resources
            .Select(r => new SolverResource(r.Id, r.Kind, r.Capacity,
                r.Availability.Select(w => new SolverWindow(w.Day, w.Start, w.End)).ToList()))
            .ToList();

        var solverActivities = activities
            .Select(a => new SolverActivity(a.Id, a.DurationMinutes, a.Priority,
                a.Requirements.Select(r => new SolverRequirement(r.Kind, r.ResourceId, r.MinCapacity)).ToList()))
            .ToList();

        return new ProblemModel(horizon, solverResources, solverActivities);
    }

    public static List<Assignment> ToAssignments(SolutionModel solution, Guid scheduleId)
    {
        var now = DateTimeOffset.UtcNow;
        return solution.Assignments.Select(s => new Assignment
        {
            Id = Guid.NewGuid(),
            ScheduleId = scheduleId,
            ActivityId = s.ActivityId,
            Start = s.Start,
            End = s.End,
            Resources = s.ResourceIds.Select(rid => new AssignmentResource { ResourceId = rid }).ToList(),
            CreatedAt = now,
            UpdatedAt = now,
        }).ToList();
    }
}
