using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prognize.Api.Data;
using Prognize.Api.Domain;

namespace Prognize.Api.Features.Schedules;

[ApiController]
[Route("api/schedules/{scheduleId:guid}/assignments")]
[Authorize]
public class AssignmentsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AssignmentsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AssignmentDto>> Create(Guid scheduleId, UpsertAssignmentRequest request, CancellationToken ct)
    {
        if (!await _db.Schedules.AnyAsync(s => s.Id == scheduleId, ct))
            return NotFound();

        var validation = await ValidateAsync(request, ct);
        if (validation is not null)
            return validation;

        var now = DateTimeOffset.UtcNow;
        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            ScheduleId = scheduleId,
            ActivityId = request.ActivityId,
            Start = request.Start,
            End = request.End,
            Resources = request.ResourceIds.Distinct().Select(rid => new AssignmentResource { ResourceId = rid }).ToList(),
            CreatedAt = now,
            UpdatedAt = now,
        };

        _db.Assignments.Add(assignment);
        await TouchScheduleAsync(scheduleId, ct);
        await _db.SaveChangesAsync(ct);

        var dto = await LoadDtoAsync(scheduleId, assignment.Id, ct);
        return CreatedAtAction(nameof(SchedulesController.GetById), "Schedules", new { id = scheduleId }, dto);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AssignmentDto>> Update(Guid scheduleId, Guid id, UpsertAssignmentRequest request, CancellationToken ct)
    {
        var assignment = await _db.Assignments
            .Include(a => a.Resources)
            .FirstOrDefaultAsync(a => a.Id == id && a.ScheduleId == scheduleId, ct);

        if (assignment is null)
            return NotFound();

        var validation = await ValidateAsync(request, ct);
        if (validation is not null)
            return validation;

        assignment.ActivityId = request.ActivityId;
        assignment.Start = request.Start;
        assignment.End = request.End;
        assignment.UpdatedAt = DateTimeOffset.UtcNow;

        var wanted = request.ResourceIds.Distinct().ToHashSet();
        assignment.Resources.RemoveAll(ar => !wanted.Contains(ar.ResourceId));
        foreach (var rid in wanted.Where(rid => assignment.Resources.All(ar => ar.ResourceId != rid)))
            assignment.Resources.Add(new AssignmentResource { ResourceId = rid });

        await TouchScheduleAsync(scheduleId, ct);
        await _db.SaveChangesAsync(ct);

        return Ok(await LoadDtoAsync(scheduleId, id, ct));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid scheduleId, Guid id, CancellationToken ct)
    {
        var assignment = await _db.Assignments.FirstOrDefaultAsync(a => a.Id == id && a.ScheduleId == scheduleId, ct);
        if (assignment is null)
            return NotFound();

        _db.Assignments.Remove(assignment);
        await TouchScheduleAsync(scheduleId, ct);
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    private async Task<ActionResult?> ValidateAsync(UpsertAssignmentRequest request, CancellationToken ct)
    {
        if (request.Start.Kind != DateTimeKind.Unspecified || request.End.Kind != DateTimeKind.Unspecified)
            return Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "Start et End doivent être des heures locales sans fuseau (ex: 2026-09-14T08:00:00).");

        if (request.End <= request.Start)
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "La fin doit être après le début.");

        if (!await _db.Activities.AnyAsync(a => a.Id == request.ActivityId, ct))
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Activité introuvable.");

        var ids = request.ResourceIds.Distinct().ToList();
        var known = await _db.Resources.CountAsync(r => ids.Contains(r.Id), ct);
        if (known != ids.Count)
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Une des ressources est introuvable.");

        return null;
    }

    private async Task TouchScheduleAsync(Guid scheduleId, CancellationToken ct)
    {
        var schedule = await _db.Schedules.FirstAsync(s => s.Id == scheduleId, ct);
        schedule.UpdatedAt = DateTimeOffset.UtcNow;
    }

    private async Task<AssignmentDto> LoadDtoAsync(Guid scheduleId, Guid assignmentId, CancellationToken ct)
    {
        var all = await _db.Assignments
            .AsNoTracking()
            .Where(a => a.ScheduleId == scheduleId)
            .Include(a => a.Activity)
            .Include(a => a.Resources).ThenInclude(ar => ar.Resource)
            .ToListAsync(ct);

        var target = all.First(a => a.Id == assignmentId);
        return ScheduleMapper.ToAssignmentDto(target, all);
    }
}
