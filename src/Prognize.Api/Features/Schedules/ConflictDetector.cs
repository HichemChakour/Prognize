using Prognize.Api.Domain;

namespace Prognize.Api.Features.Schedules;

public enum ConflictType
{
    ResourceDoubleBooked,
    ResourceUnavailable,
    CapacityInsufficient,
    RequirementUnmet,
    DurationMismatch
}

public record Conflict(ConflictType Type, string Message, Guid? ResourceId = null);

/// <summary>
/// Détection de conflits d'une affectation. Pure : aucune dépendance à EF,
/// tout ce dont elle a besoin lui est passé en paramètre.
/// </summary>
public static class ConflictDetector
{
    public static IReadOnlyList<Conflict> Detect(Assignment candidate, IEnumerable<Assignment> others)
    {
        var activity = candidate.Activity
            ?? throw new ArgumentException("candidate.Activity doit être chargé.", nameof(candidate));

        var resources = candidate.Resources
            .Select(ar => ar.Resource ?? throw new ArgumentException("candidate.Resources[].Resource doit être chargé.", nameof(candidate)))
            .ToList();

        var conflicts = new List<Conflict>();

        CheckDuration(candidate, activity, conflicts);
        CheckRequirements(activity, resources, conflicts);
        CheckAvailability(candidate, resources, conflicts);
        CheckDoubleBooking(candidate, resources, others, conflicts);

        return conflicts;
    }

    private static void CheckDuration(Assignment candidate, Activity activity, List<Conflict> conflicts)
    {
        var actual = (int)(candidate.End - candidate.Start).TotalMinutes;
        if (actual != activity.DurationMinutes)
        {
            conflicts.Add(new Conflict(
                ConflictType.DurationMismatch,
                $"Durée de {actual} min au lieu des {activity.DurationMinutes} min prévues."));
        }
    }

    private static void CheckRequirements(Activity activity, List<Resource> resources, List<Conflict> conflicts)
    {
        foreach (var req in activity.Requirements)
        {
            var candidates = resources.Where(r => r.Kind == req.Kind).ToList();

            if (req.ResourceId is { } pinnedId)
            {
                if (candidates.All(r => r.Id != pinnedId))
                {
                    conflicts.Add(new Conflict(
                        ConflictType.RequirementUnmet,
                        $"La ressource '{req.Kind}' imposée par l'activité n'est pas affectée.",
                        pinnedId));
                }
                continue;
            }

            if (candidates.Count == 0)
            {
                conflicts.Add(new Conflict(
                    ConflictType.RequirementUnmet,
                    $"Aucune ressource de type '{req.Kind}' n'est affectée."));
                continue;
            }

            if (req.MinCapacity is { } min)
            {
                foreach (var r in candidates.Where(r => (r.Capacity ?? 0) < min))
                {
                    conflicts.Add(new Conflict(
                        ConflictType.CapacityInsufficient,
                        $"{r.Name} : capacité {r.Capacity?.ToString() ?? "inconnue"}, il en faut {min}.",
                        r.Id));
                }
            }
        }
    }

    private static void CheckAvailability(Assignment candidate, List<Resource> resources, List<Conflict> conflicts)
    {
        var day = candidate.Start.DayOfWeek;
        var start = TimeOnly.FromTimeSpan(candidate.Start.TimeOfDay);
        var end = TimeOnly.FromTimeSpan(candidate.End.TimeOfDay);
        var sameDay = candidate.Start.Date == candidate.End.Date;

        foreach (var r in resources.Where(r => r.Availability.Count > 0))
        {
            var covered = sameDay && r.Availability.Any(w => w.Day == day && w.Start <= start && end <= w.End);
            if (!covered)
            {
                conflicts.Add(new Conflict(
                    ConflictType.ResourceUnavailable,
                    $"{r.Name} n'est pas disponible le {day} de {start:HH\\:mm} à {end:HH\\:mm}.",
                    r.Id));
            }
        }
    }

    private static void CheckDoubleBooking(
        Assignment candidate, List<Resource> resources, IEnumerable<Assignment> others, List<Conflict> conflicts)
    {
        var resourceIds = resources.Select(r => r.Id).ToHashSet();

        foreach (var other in others)
        {
            if (other.Id == candidate.Id)
                continue;

            var overlaps = candidate.Start < other.End && other.Start < candidate.End;
            if (!overlaps)
                continue;

            foreach (var shared in other.Resources.Where(ar => resourceIds.Contains(ar.ResourceId)))
            {
                var name = resources.First(r => r.Id == shared.ResourceId).Name;
                var otherName = other.Activity?.Name ?? "une autre activité";
                conflicts.Add(new Conflict(
                    ConflictType.ResourceDoubleBooked,
                    $"{name} est déjà prise par {otherName} de {other.Start:HH\\:mm} à {other.End:HH\\:mm}.",
                    shared.ResourceId));
            }
        }
    }
}
