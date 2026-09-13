using System.Net;
using System.Net.Http.Json;
using Prognize.Api.Domain.Enums;
using Prognize.Api.Features.Activities;
using Prognize.Api.Features.Resources;
using Prognize.Tests.Support;

namespace Prognize.Tests;

public class ActivitiesApiTests : IAsyncLifetime
{
    private readonly ApiFactory _factory = new();
    private HttpClient _adminA = null!;
    private HttpClient _adminB = null!;

    public async Task InitializeAsync()
    {
        await _factory.Db.MigrateAsync();
        _adminA = await _factory.RegisterAdminAsync("Lycée A", "admin@a.test");
        _adminB = await _factory.RegisterAdminAsync("Lycée B", "admin@b.test");
    }

    public async Task DisposeAsync() => await _factory.DisposeAsync();

    private static async Task<ResourceDto> CreateResourceAsync(HttpClient client, string name, string kind, int? capacity = null)
    {
        var response = await client.PostAsJsonAsync("/api/resources",
            new CreateResourceRequest(name, kind, capacity, null), Json.Options);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ResourceDto>(Json.Options))!;
    }

    [Fact]
    public async Task Creer_une_activite_avec_des_requirements_typés()
    {
        var dupont = await CreateResourceAsync(_adminA, "Mme Dupont", "teacher");

        var request = new CreateActivityRequest("Maths 3A", 55, 4,
        [
            new ResourceRequirementDto("teacher", dupont.Id, null),
            new ResourceRequirementDto("room", null, 30),
        ]);

        var created = await _adminA.PostAsJsonAsync("/api/activities", request, Json.Options);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        var dto = (await created.Content.ReadFromJsonAsync<ActivityDto>(Json.Options))!;
        Assert.Equal(2, dto.Requirements.Count);
        Assert.Equal(dupont.Id, dto.Requirements[0].ResourceId);
        Assert.Equal(30, dto.Requirements[1].MinCapacity);
        Assert.Equal(ActivityStatus.Active, dto.Status);

        var fetched = (await _adminA.GetFromJsonAsync<ActivityDto>($"/api/activities/{dto.Id}", Json.Options))!;
        Assert.Equal(2, fetched.Requirements.Count);
    }

    [Fact]
    public async Task Referencer_la_ressource_d_un_autre_tenant_est_refuse()
    {
        var resourceOfB = await CreateResourceAsync(_adminB, "Salle B1", "room", 20);

        var request = new CreateActivityRequest("Intrusion", 60, 3,
            [new ResourceRequirementDto("room", resourceOfB.Id, null)]);

        var response = await _adminA.PostAsJsonAsync("/api/activities", request, Json.Options);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Referencer_une_ressource_avec_le_mauvais_kind_est_refuse()
    {
        var room = await CreateResourceAsync(_adminA, "A101", "room", 30);

        var request = new CreateActivityRequest("Maths", 60, 3,
            [new ResourceRequirementDto("teacher", room.Id, null)]);

        var response = await _adminA.PostAsJsonAsync("/api/activities", request, Json.Options);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Une_organisation_ne_voit_pas_les_activites_d_une_autre()
    {
        await _adminA.PostAsJsonAsync("/api/activities", new CreateActivityRequest("Maths", 60, 3, []), Json.Options);

        var listB = (await _adminB.GetFromJsonAsync<List<ActivityDto>>("/api/activities", Json.Options))!;

        Assert.Empty(listB);
    }

    [Fact]
    public async Task Duree_ou_priorite_hors_bornes_renvoie_400()
    {
        var zeroDuration = await _adminA.PostAsJsonAsync("/api/activities", new CreateActivityRequest("X", 0, 3, []), Json.Options);
        var priority9 = await _adminA.PostAsJsonAsync("/api/activities", new CreateActivityRequest("Y", 60, 9, []), Json.Options);

        Assert.Equal(HttpStatusCode.BadRequest, zeroDuration.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, priority9.StatusCode);
    }

    [Fact]
    public async Task Update_remplace_les_requirements_et_le_statut()
    {
        var created = await _adminA.PostAsJsonAsync("/api/activities",
            new CreateActivityRequest("Maths", 60, 3, [new ResourceRequirementDto("room", null, 30)]), Json.Options);
        var dto = (await created.Content.ReadFromJsonAsync<ActivityDto>(Json.Options))!;

        var update = new UpdateActivityRequest("Maths avancées", 90, 5, [], ActivityStatus.Inactive);
        var response = await _adminA.PutAsJsonAsync($"/api/activities/{dto.Id}", update, Json.Options);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var fetched = (await _adminA.GetFromJsonAsync<ActivityDto>($"/api/activities/{dto.Id}", Json.Options))!;
        Assert.Equal("Maths avancées", fetched.Name);
        Assert.Equal(90, fetched.DurationMinutes);
        Assert.Empty(fetched.Requirements);
        Assert.Equal(ActivityStatus.Inactive, fetched.Status);
    }

    [Fact]
    public async Task Les_disponibilites_d_une_ressource_font_l_aller_retour()
    {
        var request = new CreateResourceRequest("Mme Dupont", "teacher", null, null,
        [
            new AvailabilityWindowDto(DayOfWeek.Tuesday, new TimeOnly(13, 0), new TimeOnly(17, 0)),
            new AvailabilityWindowDto(DayOfWeek.Monday, new TimeOnly(8, 0), new TimeOnly(12, 0)),
        ]);

        var created = await _adminA.PostAsJsonAsync("/api/resources", request, Json.Options);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        var dto = (await created.Content.ReadFromJsonAsync<ResourceDto>(Json.Options))!;
        var fetched = (await _adminA.GetFromJsonAsync<ResourceDto>($"/api/resources/{dto.Id}", Json.Options))!;

        Assert.Equal(2, fetched.Availability.Count);
        Assert.Equal(DayOfWeek.Monday, fetched.Availability[0].Day);
        Assert.Equal(new TimeOnly(8, 0), fetched.Availability[0].Start);
        Assert.Equal(DayOfWeek.Tuesday, fetched.Availability[1].Day);
    }

    [Fact]
    public async Task Une_fenetre_de_disponibilite_inversee_renvoie_400()
    {
        var request = new CreateResourceRequest("Salle", "room", 10, null,
            [new AvailabilityWindowDto(DayOfWeek.Monday, new TimeOnly(12, 0), new TimeOnly(8, 0))]);

        var response = await _adminA.PostAsJsonAsync("/api/resources", request, Json.Options);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
