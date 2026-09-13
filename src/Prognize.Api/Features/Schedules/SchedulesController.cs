using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prognize.Api.Data;
using Prognize.Api.Domain;
using Prognize.Api.Domain.Enums;
using Prognize.Solver;
using Prognize.Solver.Model;
using System.Text.Json;

namespace Prognize.Api.Features.Schedules;

[ApiController]
[Route("api/schedules")]
[Authorize]
public class SchedulesController : ControllerBase
{
    private readonly AppDbContext _db;

    public SchedulesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ScheduleSummaryDto>>> GetAll(CancellationToken ct)
    {
        var schedules = await _db.Schedules
            .AsNoTracking()
            .OrderByDescending(s => s.UpdatedAt)
            .Select(s => new ScheduleSummaryDto(
                s.Id, s.Name, s.Status, s.Score, s.Assignments.Count, s.CreatedAt, s.UpdatedAt))
            .ToListAsync(ct);

        return Ok(schedules);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ScheduleDetailDto>> GetById(Guid id, CancellationToken ct)
    {
        var schedule = await _db.Schedules
            .AsNoTracking()
            .Include(s => s.Assignments).ThenInclude(a => a.Activity)
            .Include(s => s.Assignments).ThenInclude(a => a.Resources).ThenInclude(ar => ar.Resource)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (schedule is null)
            return NotFound();

        return Ok(ScheduleMapper.ToDetailDto(schedule));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ScheduleSummaryDto>> Create(CreateScheduleRequest request, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var schedule = new Schedule
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
        };

        _db.Schedules.Add(schedule);
        await _db.SaveChangesAsync(ct);

        var dto = new ScheduleSummaryDto(schedule.Id, schedule.Name, schedule.Status, schedule.Score, 0, schedule.CreatedAt, schedule.UpdatedAt);
        return CreatedAtAction(nameof(GetById), new { id = schedule.Id }, dto);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, UpdateScheduleRequest request, CancellationToken ct)
    {
        var schedule = await _db.Schedules.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (schedule is null)
            return NotFound();

        schedule.Name = request.Name.Trim();
        schedule.Status = request.Status;
        schedule.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var schedule = await _db.Schedules.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (schedule is null)
            return NotFound();

        _db.Schedules.Remove(schedule);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>
    /// Génère les affectations du planning avec le solveur CP-SAT.
    /// REMPLACE les affectations existantes.
    /// </summary>
    [HttpPost("{id:guid}/solve")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<ScheduleDetailDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ScheduleDetailDto>> Solve(Guid id, SolveRequest request, CancellationToken ct)
    {
        if (request.DayStart is { } ds && request.DayEnd is { } de && de <= ds)
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "DayEnd doit être après DayStart.");

        var schedule = await _db.Schedules
            .Include(s => s.Assignments)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (schedule is null)
            return NotFound();

        var resources = await _db.Resources.AsNoTracking()
            .Where(r => r.Status == ResourceStatus.Active).ToListAsync(ct);
        var activities = await _db.Activities.AsNoTracking()
            .Where(a => a.Status == ActivityStatus.Active).ToListAsync(ct);

        var problem = SolverBridge.ToProblem(request, resources, activities);
        var solution = await Task.Run(
            () => CpSatScheduler.Solve(problem, new SolverOptions(TimeLimitSeconds: request.TimeLimitSeconds)), ct);

        _db.Assignments.RemoveRange(schedule.Assignments);
        _db.Assignments.AddRange(SolverBridge.ToAssignments(solution, schedule.Id));
        schedule.Score = solution.Score;
        schedule.Metrics = JsonSerializer.SerializeToElement(new
        {
            outcome = solution.Outcome.ToString(),
            solution.Metrics.ActivityCount,
            solution.Metrics.PlacedCount,
            solution.Metrics.UnplacedCount,
            unplacedActivityIds = solution.UnplacedActivityIds,
            solution.Metrics.WallTimeSeconds,
            solution.Metrics.Branches,
            solution.Metrics.Conflicts,
            solvedAt = DateTimeOffset.UtcNow,
        }, JsonOptions);
        schedule.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);

        return await GetById(id, ct);
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
}
