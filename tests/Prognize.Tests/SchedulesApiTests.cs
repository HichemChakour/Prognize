using System.Net;
using System.Net.Http.Json;
using Prognize.Api.Features.Activities;
using Prognize.Api.Features.Resources;
using Prognize.Api.Features.Schedules;
using Prognize.Tests.Support;

namespace Prognize.Tests;

public class SchedulesApiTests : IAsyncLifetime
{
    private static readonly DateTime Monday8h = new(2026, 9, 14, 8, 0, 0, DateTimeKind.Unspecified);

    private readonly ApiFactory _factory = new();
    private HttpClient _adminA = null!;
    private HttpClient _adminB = null!;
    private ResourceDto _room = null!;
    private ResourceDto _dupont = null!;
    private ActivityDto _maths = null!;
    private ScheduleSummaryDto _schedule = null!;

    public async Task InitializeAsync()
    {
        await _factory.Db.MigrateAsync();
        _adminA = await _factory.RegisterAdminAsync("Lycée A", "admin@a.test");
        _adminB = await _factory.RegisterAdminAsync("Lycée B", "admin@b.test");

        _room = await PostAsync<ResourceDto>(_adminA, "/api/resources", new CreateResourceRequest("A101", "room", 30, null));
        _dupont = await PostAsync<ResourceDto>(_adminA, "/api/resources", new CreateResourceRequest("Dupont", "teacher", null, null));
        _maths = await PostAsync<ActivityDto>(_adminA, "/api/activities", new CreateActivityRequest("Maths", 60, 3,
        [
            new ResourceRequirementDto("teacher", _dupont.Id, null),
            new ResourceRequirementDto("room", null, 20),
        ]));
        _schedule = await PostAsync<ScheduleSummaryDto>(_adminA, "/api/schedules", new CreateScheduleRequest("Semaine 38"));
    }

    public async Task DisposeAsync() => await _factory.DisposeAsync();

    private static async Task<T> PostAsync<T>(HttpClient client, string url, object body)
    {
        var response = await client.PostAsJsonAsync(url, body, Json.Options);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>(Json.Options))!;
    }

    private string AssignmentsUrl => $"/api/schedules/{_schedule.Id}/assignments";

    [Fact]
    public async Task Placer_une_activite_sans_conflit()
    {
        var request = new UpsertAssignmentRequest(_maths.Id, Monday8h, Monday8h.AddHours(1), [_room.Id, _dupont.Id]);

        var response = await _adminA.PostAsJsonAsync(AssignmentsUrl, request, Json.Options);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var dto = (await response.Content.ReadFromJsonAsync<AssignmentDto>(Json.Options))!;
        Assert.Empty(dto.Conflicts);
        Assert.Equal(2, dto.Resources.Count);
        Assert.Equal("Maths", dto.ActivityName);
    }

    [Fact]
    public async Task Le_detail_du_planning_expose_les_conflits()
    {
        await PostAsync<AssignmentDto>(_adminA, AssignmentsUrl,
            new UpsertAssignmentRequest(_maths.Id, Monday8h, Monday8h.AddHours(1), [_room.Id, _dupont.Id]));
        var second = await PostAsync<AssignmentDto>(_adminA, AssignmentsUrl,
            new UpsertAssignmentRequest(_maths.Id, Monday8h.AddMinutes(30), Monday8h.AddMinutes(90), [_room.Id, _dupont.Id]));

        Assert.Equal(2, second.Conflicts.Count);
        Assert.All(second.Conflicts, c => Assert.Equal(ConflictType.ResourceDoubleBooked, c.Type));

        var detail = (await _adminA.GetFromJsonAsync<ScheduleDetailDto>($"/api/schedules/{_schedule.Id}", Json.Options))!;
        Assert.Equal(2, detail.Assignments.Count);
        Assert.Equal(4, detail.ConflictCount);
    }

    [Fact]
    public async Task Besoin_non_couvert_est_signale_mais_l_affectation_est_creee()
    {
        var dto = await PostAsync<AssignmentDto>(_adminA, AssignmentsUrl,
            new UpsertAssignmentRequest(_maths.Id, Monday8h, Monday8h.AddHours(1), [_room.Id]));

        var c = Assert.Single(dto.Conflicts);
        Assert.Equal(ConflictType.RequirementUnmet, c.Type);
        Assert.Equal(_dupont.Id, c.ResourceId);
    }

    [Fact]
    public async Task Fin_avant_debut_renvoie_400()
    {
        var response = await _adminA.PostAsJsonAsync(AssignmentsUrl,
            new UpsertAssignmentRequest(_maths.Id, Monday8h, Monday8h.AddHours(-1), []), Json.Options);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Ressource_d_un_autre_tenant_est_refusee()
    {
        var roomOfB = await PostAsync<ResourceDto>(_adminB, "/api/resources", new CreateResourceRequest("B1", "room", 30, null));

        var response = await _adminA.PostAsJsonAsync(AssignmentsUrl,
            new UpsertAssignmentRequest(_maths.Id, Monday8h, Monday8h.AddHours(1), [roomOfB.Id]), Json.Options);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Planning_d_un_autre_tenant_est_invisible()
    {
        var response = await _adminB.GetAsync($"/api/schedules/{_schedule.Id}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var intrusion = await _adminB.PostAsJsonAsync(AssignmentsUrl,
            new UpsertAssignmentRequest(_maths.Id, Monday8h, Monday8h.AddHours(1), []), Json.Options);
        Assert.Equal(HttpStatusCode.NotFound, intrusion.StatusCode);
    }

    [Fact]
    public async Task Modifier_puis_supprimer_une_affectation()
    {
        var created = await PostAsync<AssignmentDto>(_adminA, AssignmentsUrl,
            new UpsertAssignmentRequest(_maths.Id, Monday8h, Monday8h.AddHours(1), [_room.Id]));

        var update = new UpsertAssignmentRequest(_maths.Id, Monday8h.AddHours(2), Monday8h.AddHours(3), [_room.Id, _dupont.Id]);
        var updated = await _adminA.PutAsJsonAsync($"{AssignmentsUrl}/{created.Id}", update, Json.Options);
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);

        var dto = (await updated.Content.ReadFromJsonAsync<AssignmentDto>(Json.Options))!;
        Assert.Equal(Monday8h.AddHours(2), dto.Start);
        Assert.Equal(2, dto.Resources.Count);
        Assert.Empty(dto.Conflicts);

        var deleted = await _adminA.DeleteAsync($"{AssignmentsUrl}/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);

        var detail = (await _adminA.GetFromJsonAsync<ScheduleDetailDto>($"/api/schedules/{_schedule.Id}", Json.Options))!;
        Assert.Empty(detail.Assignments);
    }

    [Fact]
    public async Task Supprimer_un_planning_supprime_ses_affectations()
    {
        await PostAsync<AssignmentDto>(_adminA, AssignmentsUrl,
            new UpsertAssignmentRequest(_maths.Id, Monday8h, Monday8h.AddHours(1), [_room.Id]));

        var deleted = await _adminA.DeleteAsync($"/api/schedules/{_schedule.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);

        var list = (await _adminA.GetFromJsonAsync<List<ScheduleSummaryDto>>("/api/schedules", Json.Options))!;
        Assert.Empty(list);
    }
}
