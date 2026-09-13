namespace Prognize.Solver.Model;

/// <summary>Fenêtre hebdomadaire de disponibilité d'une ressource.</summary>
public record SolverWindow(DayOfWeek Day, TimeOnly Start, TimeOnly End);

public record SolverResource(
    Guid Id,
    string Kind,
    int? Capacity,
    IReadOnlyList<SolverWindow> Availability);

public record SolverRequirement(string Kind, Guid? ResourceId, int? MinCapacity);

public record SolverActivity(
    Guid Id,
    int DurationMinutes,
    int Priority,
    IReadOnlyList<SolverRequirement> Requirements);

/// <summary>
/// La période à planifier : une semaine donnée, les jours ouverts et les heures d'ouverture.
/// SlotMinutes est la granularité du solveur (une activité commence sur un multiple de SlotMinutes).
/// </summary>
public record SolverHorizon(
    DateOnly WeekStart,
    IReadOnlyList<DayOfWeek> Days,
    TimeOnly DayStart,
    TimeOnly DayEnd,
    int SlotMinutes = 5);

public record ProblemModel(
    SolverHorizon Horizon,
    IReadOnlyList<SolverResource> Resources,
    IReadOnlyList<SolverActivity> Activities);

public record SolverOptions(double TimeLimitSeconds = 10, int Workers = 8);
