using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prognize.Api.Data;
using Prognize.Api.Domain;

namespace Prognize.Api.Features.Resources;

[ApiController]
[Route("api/resources")]
[Authorize]
public class ResourcesController : ControllerBase
{
    private readonly AppDbContext _db;

    public ResourcesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ResourceDto>>> GetAll([FromQuery] string? kind, CancellationToken ct)
    {
        var query = _db.Resources.AsNoTracking();

        if (kind is not null)
            query = query.Where(r => r.Kind == kind);

        var resources = await query
            .OrderBy(r => r.Kind)
            .ThenBy(r => r.Name)
            .ToListAsync(ct);

        return Ok(resources.Select(ToDto).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ResourceDto>> GetById(Guid id, CancellationToken ct)
    {
        var resource = await _db.Resources
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (resource is null)
            return NotFound();

        return Ok(ToDto(resource));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ResourceDto>> Create(CreateResourceRequest request, CancellationToken ct)
    {
        var name = request.Name.Trim();
        var kind = request.Kind.Trim().ToLowerInvariant();

        if (await NameTakenAsync(kind, name, excludeId: null, ct))
            return Problem(statusCode: StatusCodes.Status409Conflict, title: $"Une ressource '{name}' de type '{kind}' existe déjà.");

        if (FindInvalidWindow(request.Availability) is { } invalid)
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: invalid);

        var now = DateTimeOffset.UtcNow;
        var resource = new Resource
        {
            Id = Guid.NewGuid(),
            Name = name,
            Kind = kind,
            Capacity = request.Capacity,
            Attributes = request.Attributes,
            Availability = ToWindows(request.Availability),
            CreatedAt = now,
            UpdatedAt = now,
        };

        _db.Resources.Add(resource);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = resource.Id }, ToDto(resource));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, UpdateResourceRequest request, CancellationToken ct)
    {
        var resource = await _db.Resources.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (resource is null)
            return NotFound();

        var name = request.Name.Trim();
        var kind = request.Kind.Trim().ToLowerInvariant();

        if (await NameTakenAsync(kind, name, excludeId: id, ct))
            return Problem(statusCode: StatusCodes.Status409Conflict, title: $"Une ressource '{name}' de type '{kind}' existe déjà.");

        if (FindInvalidWindow(request.Availability) is { } invalid)
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: invalid);

        resource.Name = name;
        resource.Kind = kind;
        resource.Capacity = request.Capacity;
        resource.Status = request.Status;
        resource.Attributes = request.Attributes;
        resource.Availability = ToWindows(request.Availability);
        resource.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var resource = await _db.Resources.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (resource is null)
            return NotFound();

        _db.Resources.Remove(resource);
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    private Task<bool> NameTakenAsync(string kind, string name, Guid? excludeId, CancellationToken ct) =>
        _db.Resources.AnyAsync(r => r.Kind == kind && r.Name == name && r.Id != excludeId, ct);

    private static string? FindInvalidWindow(IReadOnlyList<AvailabilityWindowDto>? windows)
    {
        if (windows is null)
            return null;

        foreach (var w in windows)
        {
            if (w.End <= w.Start)
                return $"Fenêtre invalide le {w.Day} : la fin ({w.End}) doit être après le début ({w.Start}).";
        }

        return null;
    }

    private static List<AvailabilityWindow> ToWindows(IReadOnlyList<AvailabilityWindowDto>? dtos) =>
        (dtos ?? [])
            .OrderBy(w => w.Day).ThenBy(w => w.Start)
            .Select(w => new AvailabilityWindow { Day = w.Day, Start = w.Start, End = w.End })
            .ToList();

    private static ResourceDto ToDto(Resource r) =>
        new(r.Id, r.Name, r.Kind, r.Capacity, r.Status, r.Attributes,
            r.Availability.Select(w => new AvailabilityWindowDto(w.Day, w.Start, w.End)).ToList(),
            r.CreatedAt, r.UpdatedAt);
}
