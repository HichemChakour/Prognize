using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Prognize.Api.Features.Activities;
using Prognize.Api.Features.Auth;
using Prognize.Api.Features.Resources;
using Prognize.Api.Features.Schedules;
using Prognize.Tests.Support;

namespace Prognize.Tests;

public class SolveApiTests : IAsyncLifetime
{
    private readonly ApiFactory _factory = new();
    private HttpClient _admin = null!;
    private ScheduleSummaryDto _schedule = null!;

    public async Task InitializeAsync()
    {
        await _factory.Db.MigrateAsync();
        _admin = await _factory.RegisterAdminAsync("Lycée A", "admin@a.test");

        // 2 salles (une petite), 2 profs (Dupont dispo mardi après-midi seulement), 4 activités.
        var a101 = await Post<ResourceDto>("/api/resources", new CreateResourceRequest("A101", "room", 35, null));
        var b12 = await Post<ResourceDto>("/api/resources", new CreateResourceRequest("B12", "room", 15, null));
        var dupont = await Post<ResourceDto>("/api/resources", new CreateResourceRequest("Dupont", "teacher", null, null,
            [new AvailabilityWindowDto(DayOfWeek.Tuesday, new TimeOnly(13, 0), new TimeOnly(18, 0))]));
        var martin = await Post<ResourceDto>("/api/resources", new CreateResourceRequest("Martin", "teacher", null, null));

        await Post<ActivityDto>("/api/activities", new CreateActivityRequest("Maths 3A", 60, 5,
            [new ResourceRequirementDto("teacher", dupont.Id, null), new ResourceRequirementDto("room", null, 30)]));
        await Post<ActivityDto>("/api/activities", new CreateActivityRequest("Maths 3B", 60, 4,
            [new ResourceRequirementDto("teacher", dupont.Id, null), new ResourceRequirementDto("room", null, 30)]));
        await Post<ActivityDto>("/api/activities", new CreateActivityRequest("Physique 3A", 90, 3,
            [new ResourceRequirementDto("teacher", martin.Id, null), new ResourceRequirementDto("room", null, 30)]));
        await Post<ActivityDto>("/api/activities", new CreateActivityRequest("Soutien", 45, 2,
            [new ResourceRequirementDto("teacher", null, null), new ResourceRequirementDto("room", null, null)]));

        _schedule = await Post<ScheduleSummaryDto>("/api/schedules", new CreateScheduleRequest("Semaine 38"));
    }

    public async Task DisposeAsync() => await _factory.DisposeAsync();

    private async Task<T> Post<T>(string url, object body)
    {
        var response = await _admin.PostAsJsonAsync(url, body, Json.Options);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>(Json.Options))!;
    }

    [Fact]
    public async Task Le_solveur_place_tout_sans_aucun_conflit()
    {
        var request = new SolveRequest(new DateOnly(2026, 9, 14), TimeLimitSeconds: 5);

        var response = await _admin.PostAsJsonAsync($"/api/schedules/{_schedule.Id}/solve", request, Json.Options);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var detail = (await response.Content.ReadFromJsonAsync<ScheduleDetailDto>(Json.Options))!;

        Assert.Equal(4, detail.Assignments.Count);
        Assert.Equal(0, detail.ConflictCount);
        Assert.Equal(100, detail.Score);
        Assert.NotNull(detail.Metrics);
        Assert.Equal("Optimal", detail.Metrics.Value.GetProperty("outcome").GetString());

        var dupontLessons = detail.Assignments.Where(a => a.ActivityName.StartsWith("Maths")).ToList();
        Assert.All(dupontLessons, a => Assert.Equal(DayOfWeek.Tuesday, a.Start.DayOfWeek));
        Assert.All(dupontLessons, a => Assert.True(a.Start.TimeOfDay >= TimeSpan.FromHours(13)));
        Assert.All(detail.Assignments, a => Assert.All(a.Resources.Where(r => r.Kind == "room"), r => Assert.True(r.Capacity >= 15)));
    }

    [Fact]
    public async Task Resoudre_remplace_les_affectations_manuelles()
    {
        var activity = (await _admin.GetFromJsonAsync<List<ActivityDto>>("/api/activities", Json.Options))![0];
        await Post<AssignmentDto>($"/api/schedules/{_schedule.Id}/assignments",
            new UpsertAssignmentRequest(activity.Id, new DateTime(2026, 9, 14, 8, 0, 0), new DateTime(2026, 9, 14, 9, 0, 0), []));

        var detail = await Post<ScheduleDetailDto>($"/api/schedules/{_schedule.Id}/solve", new SolveRequest(new DateOnly(2026, 9, 14), TimeLimitSeconds: 5));

        Assert.Equal(4, detail.Assignments.Count);
        Assert.All(detail.Assignments, a => Assert.NotEmpty(a.Resources));
    }

    [Fact]
    public async Task Horizon_trop_court_laisse_des_activites_non_placees()
    {
        // Une seule journée d'une heure : impossible de tout caser.
        var request = new SolveRequest(new DateOnly(2026, 9, 14), [DayOfWeek.Tuesday], new TimeOnly(13, 0), new TimeOnly(14, 0), 5);

        var detail = await Post<ScheduleDetailDto>($"/api/schedules/{_schedule.Id}/solve", request);

        Assert.True(detail.Assignments.Count < 4);
        Assert.Equal(0, detail.ConflictCount);
        var unplaced = detail.Metrics!.Value.GetProperty("unplacedActivityIds").GetArrayLength();
        Assert.Equal(4 - detail.Assignments.Count, unplaced);
        Assert.True(detail.Score < 100);
    }

    [Fact]
    public async Task Un_membre_ne_peut_pas_lancer_le_solveur()
    {
        var me = (await _admin.GetFromJsonAsync<CurrentUserDto>("/api/auth/me", Json.Options))!;
        var member = await _factory.CreateMemberClientAsync(me.OrganizationId, "member@a.test");

        var response = await member.PostAsJsonAsync($"/api/schedules/{_schedule.Id}/solve", new SolveRequest(new DateOnly(2026, 9, 14)), Json.Options);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
