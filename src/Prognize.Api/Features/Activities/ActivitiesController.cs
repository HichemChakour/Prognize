using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prognize.Api.Data;
using Prognize.Api.Domain;

namespace Prognize.Api.Features.Activities;

[ApiController]
[Route("api/activities")]
[Authorize]
public class ActivitiesController : ControllerBase
{
    private readonly AppDbContext _db;

    public ActivitiesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ActivityDto>>> GetAll(CancellationToken ct)
    {
        var activities = await _db.Activities
            .AsNoTracking()
            .OrderByDescending(a => a.Priority)
            .ThenBy(a => a.Name)
            .ToListAsync(ct);

        return Ok(activities.Select(ToDto).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ActivityDto>> GetById(Guid id, CancellationToken ct)
    {
        var activity = await _db.Activities
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (activity is null)
            return NotFound();

        return Ok(ToDto(activity));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ActivityDto>> Create(CreateActivityRequest request, CancellationToken ct)
    {
        var name = request.Name.Trim();

        if (await NameTakenAsync(name, excludeId: null, ct))
            return Problem(statusCode: StatusCodes.Status409Conflict, title: $"Une activité '{name}' existe déjà.");

        var invalidReference = await FindInvalidResourceReferenceAsync(request.Requirements, ct);
        if (invalidReference is not null)
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: invalidReference);

        var now = DateTimeOffset.UtcNow;
        var activity = new Activity
        {
            Id = Guid.NewGuid(),
            Name = name,
            DurationMinutes = request.DurationMinutes,
            Priority = request.Priority,
            Requirements = ToRequirements(request.Requirements),
            CreatedAt = now,
            UpdatedAt = now,
        };

        _db.Activities.Add(activity);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = activity.Id }, ToDto(activity));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, UpdateActivityRequest request, CancellationToken ct)
    {
        var activity = await _db.Activities.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (activity is null)
            return NotFound();

        var name = request.Name.Trim();

        if (await NameTakenAsync(name, excludeId: id, ct))
            return Problem(statusCode: StatusCodes.Status409Conflict, title: $"Une activité '{name}' existe déjà.");

        var invalidReference = await FindInvalidResourceReferenceAsync(request.Requirements, ct);
        if (invalidReference is not null)
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: invalidReference);

        activity.Name = name;
        activity.DurationMinutes = request.DurationMinutes;
        activity.Priority = request.Priority;
        activity.Requirements = ToRequirements(request.Requirements);
        activity.Status = request.Status;
        activity.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var activity = await _db.Activities.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (activity is null)
            return NotFound();

        _db.Activities.Remove(activity);
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    private Task<bool> NameTakenAsync(string name, Guid? excludeId, CancellationToken ct) =>
        _db.Activities.AnyAsync(a => a.Name == name && a.Id != excludeId, ct);

    /// <summary>
    /// Vérifie que chaque ResourceId référencé existe DANS LE TENANT COURANT
    /// (la requête passe par le query filter) et que son kind correspond.
    /// Renvoie un message d'erreur, ou null si tout est valide.
    /// </summary>
    private async Task<string?> FindInvalidResourceReferenceAsync(
        IReadOnlyList<ResourceRequirementDto> requirements, CancellationToken ct)
    {
        var pinned = requirements
            .Where(r => r.ResourceId is not null)
            .Select(r => new { Id = r.ResourceId!.Value, Kind = r.Kind.Trim().ToLowerInvariant() })
            .Distinct()
            .ToList();

        if (pinned.Count == 0)
            return null;

        var ids = pinned.Select(p => p.Id).ToList();
        var known = await _db.Resources
            .Where(r => ids.Contains(r.Id))
            .Select(r => new { r.Id, r.Kind })
            .ToDictionaryAsync(r => r.Id, r => r.Kind, ct);

        foreach (var p in pinned)
        {
            if (!known.TryGetValue(p.Id, out var actualKind))
                return $"La ressource {p.Id} n'existe pas.";

            if (actualKind != p.Kind)
                return $"La ressource {p.Id} est de type '{actualKind}', pas '{p.Kind}'.";
        }

        return null;
    }

    private static List<ResourceRequirement> ToRequirements(IReadOnlyList<ResourceRequirementDto> dtos) =>
        dtos.Select(r => new ResourceRequirement
        {
            Kind = r.Kind.Trim().ToLowerInvariant(),
            ResourceId = r.ResourceId,
            MinCapacity = r.MinCapacity,
        }).ToList();

    private static ActivityDto ToDto(Activity a) =>
        new(a.Id, a.Name, a.DurationMinutes, a.Priority,
            a.Requirements.Select(r => new ResourceRequirementDto(r.Kind, r.ResourceId, r.MinCapacity)).ToList(),
            a.Status, a.CreatedAt, a.UpdatedAt);
}
