namespace Prognize.Solver.Model;

public enum SolverOutcome
{
    Optimal,
    Feasible,
    Infeasible,
    Unknown
}

public record SolvedAssignment(
    Guid ActivityId,
    DateTime Start,
    DateTime End,
    IReadOnlyList<Guid> ResourceIds);

public record SolverMetrics(
    int ActivityCount,
    int PlacedCount,
    int UnplacedCount,
    double WallTimeSeconds,
    long Branches,
    long Conflicts);

public record SolutionModel(
    SolverOutcome Outcome,
    IReadOnlyList<SolvedAssignment> Assignments,
    IReadOnlyList<Guid> UnplacedActivityIds,
    double Score,
    SolverMetrics Metrics);
