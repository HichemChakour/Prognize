using Prognize.Api.Domain;
using Prognize.Api.Features.Schedules;

namespace Prognize.Tests;

public class ConflictDetectorTests
{
    private static readonly DateTime Monday8h = new(2026, 9, 14, 8, 0, 0, DateTimeKind.Unspecified);

    private static Resource Room(string name, int? capacity = 30, params AvailabilityWindow[] windows) =>
        new() { Id = Guid.NewGuid(), Name = name, Kind = "room", Capacity = capacity, Availability = [.. windows] };

    private static Resource Teacher(string name, params AvailabilityWindow[] windows) =>
        new() { Id = Guid.NewGuid(), Name = name, Kind = "teacher", Availability = [.. windows] };

    private static Activity Maths(int duration = 60, params ResourceRequirement[] reqs) =>
        new() { Id = Guid.NewGuid(), Name = "Maths", DurationMinutes = duration, Requirements = [.. reqs] };

    private static Assignment Place(Activity activity, DateTime start, int minutes, params Resource[] resources) =>
        new()
        {
            Id = Guid.NewGuid(),
            Activity = activity,
            ActivityId = activity.Id,
            Start = start,
            End = start.AddMinutes(minutes),
            Resources = resources.Select(r => new AssignmentResource { ResourceId = r.Id, Resource = r }).ToList(),
        };

    [Fact]
    public void Aucun_conflit_quand_tout_est_coherent()
    {
        var room = Room("A101");
        var a = Place(Maths(60, new ResourceRequirement { Kind = "room", MinCapacity = 20 }), Monday8h, 60, room);

        var conflicts = ConflictDetector.Detect(a, []);

        Assert.Empty(conflicts);
    }

    [Fact]
    public void Duree_differente_de_l_activite()
    {
        var a = Place(Maths(60), Monday8h, 90);

        var conflicts = ConflictDetector.Detect(a, []);

        Assert.Contains(conflicts, c => c.Type == ConflictType.DurationMismatch);
    }

    [Fact]
    public void Besoin_non_couvert_par_type()
    {
        var a = Place(Maths(60, new ResourceRequirement { Kind = "teacher" }), Monday8h, 60, Room("A101"));

        var conflicts = ConflictDetector.Detect(a, []);

        var c = Assert.Single(conflicts, c => c.Type == ConflictType.RequirementUnmet);
        Assert.Contains("teacher", c.Message);
    }

    [Fact]
    public void Besoin_non_couvert_par_ressource_imposee()
    {
        var dupont = Teacher("Dupont");
        var martin = Teacher("Martin");
        var a = Place(Maths(60, new ResourceRequirement { Kind = "teacher", ResourceId = dupont.Id }), Monday8h, 60, martin);

        var conflicts = ConflictDetector.Detect(a, []);

        var c = Assert.Single(conflicts, c => c.Type == ConflictType.RequirementUnmet);
        Assert.Equal(dupont.Id, c.ResourceId);
    }

    [Fact]
    public void Capacite_insuffisante()
    {
        var small = Room("B12", capacity: 15);
        var a = Place(Maths(60, new ResourceRequirement { Kind = "room", MinCapacity = 30 }), Monday8h, 60, small);

        var conflicts = ConflictDetector.Detect(a, []);

        var c = Assert.Single(conflicts, c => c.Type == ConflictType.CapacityInsufficient);
        Assert.Equal(small.Id, c.ResourceId);
    }

    [Fact]
    public void Ressource_hors_disponibilite()
    {
        var dupont = Teacher("Dupont", new AvailabilityWindow { Day = DayOfWeek.Monday, Start = new(13, 0), End = new(17, 0) });
        var a = Place(Maths(), Monday8h, 60, dupont);

        var conflicts = ConflictDetector.Detect(a, []);

        Assert.Contains(conflicts, c => c.Type == ConflictType.ResourceUnavailable && c.ResourceId == dupont.Id);
    }

    [Fact]
    public void Ressource_dans_sa_fenetre_de_disponibilite()
    {
        var dupont = Teacher("Dupont", new AvailabilityWindow { Day = DayOfWeek.Monday, Start = new(8, 0), End = new(12, 0) });
        var a = Place(Maths(), Monday8h, 60, dupont);

        var conflicts = ConflictDetector.Detect(a, []);

        Assert.DoesNotContain(conflicts, c => c.Type == ConflictType.ResourceUnavailable);
    }

    [Fact]
    public void Ressource_sans_fenetre_est_toujours_disponible()
    {
        var a = Place(Maths(), Monday8h.AddHours(15), 60, Teacher("Dupont"));

        Assert.DoesNotContain(ConflictDetector.Detect(a, []), c => c.Type == ConflictType.ResourceUnavailable);
    }

    [Fact]
    public void Double_reservation_sur_creneau_chevauchant()
    {
        var room = Room("A101");
        var existing = Place(Maths(), Monday8h, 60, room);
        var candidate = Place(Maths(), Monday8h.AddMinutes(30), 60, room);

        var conflicts = ConflictDetector.Detect(candidate, [existing]);

        var c = Assert.Single(conflicts, c => c.Type == ConflictType.ResourceDoubleBooked);
        Assert.Equal(room.Id, c.ResourceId);
    }

    [Fact]
    public void Creneaux_qui_se_touchent_ne_se_chevauchent_pas()
    {
        var room = Room("A101");
        var existing = Place(Maths(), Monday8h, 60, room);
        var candidate = Place(Maths(), Monday8h.AddMinutes(60), 60, room);

        Assert.DoesNotContain(ConflictDetector.Detect(candidate, [existing]), c => c.Type == ConflictType.ResourceDoubleBooked);
    }

    [Fact]
    public void Chevauchement_sans_ressource_commune_n_est_pas_un_conflit()
    {
        var existing = Place(Maths(), Monday8h, 60, Room("A101"));
        var candidate = Place(Maths(), Monday8h, 60, Room("A102"));

        Assert.Empty(ConflictDetector.Detect(candidate, [existing]));
    }

    [Fact]
    public void Une_affectation_ne_se_conflicte_pas_avec_elle_meme()
    {
        var room = Room("A101");
        var a = Place(Maths(), Monday8h, 60, room);

        Assert.Empty(ConflictDetector.Detect(a, [a]));
    }
}
