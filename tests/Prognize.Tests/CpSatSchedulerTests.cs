using Prognize.Solver;
using Prognize.Solver.Model;

namespace Prognize.Tests;

public class CpSatSchedulerTests
{
    private static readonly DateOnly Monday = new(2026, 9, 14);

    private static SolverHorizon Week(params DayOfWeek[] days) =>
        new(Monday, days.Length == 0 ? [DayOfWeek.Monday, DayOfWeek.Tuesday] : days, new TimeOnly(8, 0), new TimeOnly(18, 0));

    private static SolverResource Room(string id, int capacity = 30, params SolverWindow[] windows) =>
        new(Guid.Parse(id), "room", capacity, windows);

    private static SolverResource Teacher(string id, params SolverWindow[] windows) =>
        new(Guid.Parse(id), "teacher", null, windows);

    private static SolverActivity Activity(string id, int duration, int priority, params SolverRequirement[] reqs) =>
        new(Guid.Parse(id), duration, priority, reqs);

    private const string R1 = "11111111-1111-1111-1111-111111111111";
    private const string R2 = "22222222-2222-2222-2222-222222222222";
    private const string T1 = "33333333-3333-3333-3333-333333333333";
    private const string A1 = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";
    private const string A2 = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb";
    private const string A3 = "cccccccc-cccc-cccc-cccc-cccccccccccc";

    private static SolutionModel Solve(ProblemModel p) => CpSatScheduler.Solve(p, new SolverOptions(TimeLimitSeconds: 5));

    [Fact]
    public void Place_une_activite_dans_les_heures_d_ouverture()
    {
        var problem = new ProblemModel(Week(DayOfWeek.Monday), [Room(R1)],
            [Activity(A1, 60, 3, new SolverRequirement("room", null, null))]);

        var solution = Solve(problem);

        Assert.Equal(SolverOutcome.Optimal, solution.Outcome);
        var a = Assert.Single(solution.Assignments);
        Assert.Equal(new DateTime(2026, 9, 14), a.Start.Date);
        Assert.InRange(a.Start.TimeOfDay, TimeSpan.FromHours(8), TimeSpan.FromHours(17));
        Assert.Equal(60, (a.End - a.Start).TotalMinutes);
        Assert.Equal(Guid.Parse(R1), Assert.Single(a.ResourceIds));
        Assert.Equal(100, solution.Score);
    }

    [Fact]
    public void Deux_activites_sur_la_meme_salle_ne_se_chevauchent_pas()
    {
        var problem = new ProblemModel(Week(DayOfWeek.Monday), [Room(R1)],
        [
            Activity(A1, 120, 3, new SolverRequirement("room", null, null)),
            Activity(A2, 120, 3, new SolverRequirement("room", null, null)),
        ]);

        var solution = Solve(problem);

        Assert.Equal(2, solution.Assignments.Count);
        var (x, y) = (solution.Assignments[0], solution.Assignments[1]);
        Assert.False(x.Start < y.End && y.Start < x.End, "les deux créneaux se chevauchent");
    }

    [Fact]
    public void Respecte_les_disponibilites_du_professeur()
    {
        var dupont = Teacher(T1, new SolverWindow(DayOfWeek.Tuesday, new TimeOnly(14, 0), new TimeOnly(16, 0)));
        var problem = new ProblemModel(Week(), [Room(R1), dupont],
            [Activity(A1, 60, 3, new SolverRequirement("teacher", Guid.Parse(T1), null), new SolverRequirement("room", null, null))]);

        var solution = Solve(problem);

        var a = Assert.Single(solution.Assignments);
        Assert.Equal(DayOfWeek.Tuesday, a.Start.DayOfWeek);
        Assert.InRange(a.Start.TimeOfDay, TimeSpan.FromHours(14), TimeSpan.FromHours(15));
        Assert.Contains(Guid.Parse(T1), a.ResourceIds);
    }

    [Fact]
    public void Choisit_une_salle_assez_grande()
    {
        var problem = new ProblemModel(Week(), [Room(R1, capacity: 15), Room(R2, capacity: 40)],
            [Activity(A1, 60, 3, new SolverRequirement("room", null, 30))]);

        var solution = Solve(problem);

        var a = Assert.Single(solution.Assignments);
        Assert.Equal(Guid.Parse(R2), Assert.Single(a.ResourceIds));
    }

    [Fact]
    public void Activite_impossible_est_laissee_non_placee_sans_bloquer_les_autres()
    {
        var problem = new ProblemModel(Week(), [Room(R1)],
        [
            Activity(A1, 60, 3, new SolverRequirement("room", null, null)),
            Activity(A2, 60, 5, new SolverRequirement("teacher", null, null)),   // aucun teacher
        ]);

        var solution = Solve(problem);

        Assert.Single(solution.Assignments);
        Assert.Equal(Guid.Parse(A2), Assert.Single(solution.UnplacedActivityIds));
        Assert.Equal(1, solution.Metrics.UnplacedCount);
    }

    [Fact]
    public void Sous_contrainte_les_prioritaires_passent_en_premier()
    {
        // Une seule salle, ouverte 2h, trois activités de 1h : deux passent, la priorité 1 reste dehors.
        var horizon = new SolverHorizon(Monday, [DayOfWeek.Monday], new TimeOnly(8, 0), new TimeOnly(10, 0));
        var problem = new ProblemModel(horizon, [Room(R1)],
        [
            Activity(A1, 60, 1, new SolverRequirement("room", null, null)),
            Activity(A2, 60, 5, new SolverRequirement("room", null, null)),
            Activity(A3, 60, 4, new SolverRequirement("room", null, null)),
        ]);

        var solution = Solve(problem);

        Assert.Equal(2, solution.Assignments.Count);
        Assert.Equal(Guid.Parse(A1), Assert.Single(solution.UnplacedActivityIds));
        Assert.Equal(90, solution.Score);   // 9 / 10 du poids total
    }

    [Fact]
    public void Aucune_activite_donne_une_solution_vide()
    {
        var solution = Solve(new ProblemModel(Week(), [Room(R1)], []));

        Assert.Empty(solution.Assignments);
        Assert.Equal(100, solution.Score);
    }
}
