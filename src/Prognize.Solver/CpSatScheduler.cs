using Google.OrTools.Sat;
using Google.OrTools.Util;
using Prognize.Solver.Model;

namespace Prognize.Solver;

/// <summary>
/// Planificateur CP-SAT. Stateless : un ProblemModel entre, un SolutionModel sort.
///
/// Modélisation :
///   - le temps est découpé en créneaux de SlotMinutes depuis le lundi 00:00 de la semaine ;
///   - chaque activité a une variable "start" (créneau) dont le domaine est l'union des
///     plages horaires ouvertes (contrainte n°1 : dans les heures d'ouverture) ;
///   - chaque activité a un booléen "placed" — on ne force pas le placement, on le maximise
///     pondéré par la priorité (objectif) ;
///   - pour chaque besoin, un booléen "use" par ressource candidate (bon kind, capacité
///     suffisante, ressource imposée) ; exactement un "use" par besoin si l'activité est
///     placée (contrainte n°2 : besoins satisfaits) ;
///   - pour chaque (activité, ressource), un intervalle optionnel ; AddNoOverlap par
///     ressource (contrainte n°3 : pas de double réservation) ;
///   - si la ressource a des fenêtres de disponibilité, le start est restreint à leur
///     union quand elle est utilisée (contrainte n°4 : disponibilités).
/// </summary>
public static class CpSatScheduler
{
    public static SolutionModel Solve(ProblemModel problem, SolverOptions? options = null)
    {
        options ??= new SolverOptions();
        var horizon = problem.Horizon;
        var slot = horizon.SlotMinutes;
        var slotsPerDay = 1440 / slot;

        int ToSlot(TimeOnly t) => (t.Hour * 60 + t.Minute) / slot;
        int DayOffset(DayOfWeek day) => ((int)day - (int)DayOfWeek.Monday + 7) % 7 * slotsPerDay;

        var dayStart = ToSlot(horizon.DayStart);
        var dayEnd = ToSlot(horizon.DayEnd);

        var model = new CpModel();
        var placed = new Dictionary<Guid, BoolVar>();
        var starts = new Dictionary<Guid, IntVar>();
        var durations = new Dictionary<Guid, int>();
        var assigned = new Dictionary<(Guid Activity, Guid Resource), BoolVar>();
        var intervalsByResource = problem.Resources.ToDictionary(r => r.Id, _ => new List<IntervalVar>());
        var unplaceable = new List<Guid>();

        foreach (var activity in problem.Activities)
        {
            var durSlots = Math.Max(1, (int)Math.Ceiling(activity.DurationMinutes / (double)slot));
            durations[activity.Id] = durSlots;

            // Domaine de départ : union des [ouverture, fermeture - durée] de chaque jour ouvert.
            var openIntervals = horizon.Days
                .Select(d => (Lo: (long)(DayOffset(d) + dayStart), Hi: (long)(DayOffset(d) + dayEnd - durSlots)))
                .Where(i => i.Hi >= i.Lo)
                .ToList();

            if (openIntervals.Count == 0)
            {
                unplaceable.Add(activity.Id);
                continue;
            }

            var start = model.NewIntVarFromDomain(ToDomain(openIntervals), $"start_{activity.Id:N}");
            var isPlaced = model.NewBoolVar($"placed_{activity.Id:N}");
            starts[activity.Id] = start;
            placed[activity.Id] = isPlaced;

            var usesByResource = new Dictionary<Guid, List<BoolVar>>();

            for (var ri = 0; ri < activity.Requirements.Count; ri++)
            {
                var req = activity.Requirements[ri];
                var candidates = problem.Resources.Where(r =>
                    r.Kind == req.Kind
                    && (req.ResourceId is null || r.Id == req.ResourceId)
                    && (req.MinCapacity is null || (r.Capacity ?? 0) >= req.MinCapacity)).ToList();

                if (candidates.Count == 0)
                {
                    model.Add(isPlaced == 0);
                    continue;
                }

                var uses = new List<BoolVar>();
                foreach (var res in candidates)
                {
                    var use = model.NewBoolVar($"use_{activity.Id:N}_{ri}_{res.Id:N}");
                    uses.Add(use);
                    if (!usesByResource.TryGetValue(res.Id, out var list))
                        usesByResource[res.Id] = list = [];
                    list.Add(use);
                }

                // Exactement une ressource par besoin si l'activité est placée, aucune sinon.
                model.Add(LinearExpr.Sum(uses) == isPlaced);
            }

            foreach (var (resourceId, uses) in usesByResource)
            {
                var res = problem.Resources.First(r => r.Id == resourceId);
                var isAssigned = model.NewBoolVar($"assigned_{activity.Id:N}_{resourceId:N}");
                assigned[(activity.Id, resourceId)] = isAssigned;

                // Une ressource ne couvre qu'un seul besoin d'une même activité.
                model.Add(LinearExpr.Sum(uses) == isAssigned);

                var interval = model.NewOptionalFixedSizeIntervalVar(
                    start, durSlots, isAssigned, $"iv_{activity.Id:N}_{resourceId:N}");
                intervalsByResource[resourceId].Add(interval);

                if (res.Availability.Count > 0)
                {
                    var windows = res.Availability
                        .Select(w => (Lo: (long)(DayOffset(w.Day) + ToSlot(w.Start)), Hi: (long)(DayOffset(w.Day) + ToSlot(w.End) - durSlots)))
                        .Where(i => i.Hi >= i.Lo)
                        .ToList();

                    if (windows.Count == 0)
                        model.Add(isAssigned == 0);
                    else
                        model.AddLinearExpressionInDomain(start, ToDomain(windows)).OnlyEnforceIf(isAssigned);
                }
            }
        }

        foreach (var intervals in intervalsByResource.Values.Where(l => l.Count > 1))
            model.AddNoOverlap(intervals);

        // Objectif : placer le plus d'activités possible, les prioritaires d'abord.
        var weights = placed.Keys.Select(id => problem.Activities.First(a => a.Id == id).Priority).ToList();
        model.Maximize(LinearExpr.WeightedSum(placed.Values, weights));

        var solver = new CpSolver
        {
            StringParameters = $"max_time_in_seconds:{options.TimeLimitSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture)},num_workers:{options.Workers}",
        };
        var status = solver.Solve(model);

        var outcome = status switch
        {
            CpSolverStatus.Optimal => SolverOutcome.Optimal,
            CpSolverStatus.Feasible => SolverOutcome.Feasible,
            CpSolverStatus.Infeasible => SolverOutcome.Infeasible,
            _ => SolverOutcome.Unknown,
        };

        var result = new List<SolvedAssignment>();
        var unplaced = new List<Guid>(unplaceable);

        if (outcome is SolverOutcome.Optimal or SolverOutcome.Feasible)
        {
            var weekStart = horizon.WeekStart.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);

            foreach (var (activityId, isPlaced) in placed)
            {
                if (!solver.BooleanValue(isPlaced))
                {
                    unplaced.Add(activityId);
                    continue;
                }

                var startSlot = solver.Value(starts[activityId]);
                var startTime = weekStart.AddMinutes(startSlot * slot);
                var endTime = startTime.AddMinutes(durations[activityId] * slot);

                var resourceIds = assigned
                    .Where(kv => kv.Key.Activity == activityId && solver.BooleanValue(kv.Value))
                    .Select(kv => kv.Key.Resource)
                    .ToList();

                result.Add(new SolvedAssignment(activityId, startTime, endTime, resourceIds));
            }
        }
        else
        {
            unplaced.AddRange(placed.Keys);
        }

        var totalWeight = problem.Activities.Sum(a => a.Priority);
        var placedWeight = result.Sum(r => problem.Activities.First(a => a.Id == r.ActivityId).Priority);
        var score = totalWeight == 0 ? 100 : Math.Round(100.0 * placedWeight / totalWeight, 1);

        var metrics = new SolverMetrics(
            problem.Activities.Count, result.Count, unplaced.Count,
            Math.Round(solver.WallTime(), 3), solver.NumBranches(), solver.NumConflicts());

        return new SolutionModel(outcome, result, unplaced, score, metrics);
    }

    private static Domain ToDomain(List<(long Lo, long Hi)> intervals) =>
        Domain.FromFlatIntervals(intervals.SelectMany(i => new[] { i.Lo, i.Hi }).ToArray());
}
