using Prognize.Api.Domain;

namespace Prognize.Api.Features.Schedules;

public static class ScheduleMapper
{
    public static ScheduleDetailDto ToDetailDto(Schedule schedule)
    {
        var assignments = schedule.Assignments
            .OrderBy(a => a.Start)
            .Select(a => ToAssignmentDto(a, schedule.Assignments))
            .ToList();

        return new ScheduleDetailDto(
            schedule.Id, schedule.Name, schedule.Status, schedule.Score, schedule.Metrics,
            assignments, assignments.Sum(a => a.Conflicts.Count),
            schedule.CreatedAt, schedule.UpdatedAt);
    }

    public static AssignmentDto ToAssignmentDto(Assignment a, IEnumerable<Assignment> allInSchedule)
    {
        var conflicts = ConflictDetector.Detect(a, allInSchedule)
            .Select(c => new ConflictDto(c.Type, c.Message, c.ResourceId))
            .ToList();

        var resources = a.Resources
            .Select(ar => ar.Resource!)
            .OrderBy(r => r.Kind).ThenBy(r => r.Name)
            .Select(r => new AssignedResourceDto(r.Id, r.Name, r.Kind, r.Capacity))
            .ToList();

        return new AssignmentDto(
            a.Id, a.ScheduleId, a.ActivityId, a.Activity!.Name, a.Activity.Priority,
            a.Start, a.End, resources, conflicts);
    }
}
