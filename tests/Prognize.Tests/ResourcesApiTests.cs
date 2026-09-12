using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Prognize.Api.Domain.Enums;
using Prognize.Api.Features.Auth;
using Prognize.Api.Features.Resources;
using Prognize.Tests.Support;

namespace Prognize.Tests;

public class ResourcesApiTests : IAsyncLifetime
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

    private static CreateResourceRequest Room(string name, int capacity = 30) =>
        new(name, "room", capacity, null);

    [Fact]
    public async Task Un_admin_cree_une_ressource_et_la_retrouve()
    {
        var created = await _adminA.PostAsJsonAsync("/api/resources", Room("A101"), Json.Options);

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.NotNull(created.Headers.Location);

        var dto = (await created.Content.ReadFromJsonAsync<ResourceDto>(Json.Options))!;
        Assert.Equal("A101", dto.Name);
        Assert.Equal(ResourceStatus.Active, dto.Status);

        var list = (await _adminA.GetFromJsonAsync<List<ResourceDto>>("/api/resources", Json.Options))!;
        Assert.Single(list);
        Assert.Equal(dto.Id, list[0].Id);
    }

    [Fact]
    public async Task Une_organisation_ne_voit_pas_les_ressources_d_une_autre()
    {
        var created = await _adminA.PostAsJsonAsync("/api/resources", Room("A101"), Json.Options);
        var dto = (await created.Content.ReadFromJsonAsync<ResourceDto>(Json.Options))!;

        var listB = (await _adminB.GetFromJsonAsync<List<ResourceDto>>("/api/resources", Json.Options))!;
        Assert.Empty(listB);

        var stolen = await _adminB.GetAsync($"/api/resources/{dto.Id}");
        Assert.Equal(HttpStatusCode.NotFound, stolen.StatusCode);

        var deleteAttempt = await _adminB.DeleteAsync($"/api/resources/{dto.Id}");
        Assert.Equal(HttpStatusCode.NotFound, deleteAttempt.StatusCode);
    }

    [Fact]
    public async Task Un_membre_peut_lire_mais_pas_ecrire()
    {
        var me = (await _adminA.GetFromJsonAsync<CurrentUserDto>("/api/auth/me", Json.Options))!;
        var member = await _factory.CreateMemberClientAsync(me.OrganizationId, "member@a.test");

        await _adminA.PostAsJsonAsync("/api/resources", Room("A101"), Json.Options);

        var list = (await member.GetFromJsonAsync<List<ResourceDto>>("/api/resources", Json.Options))!;
        Assert.Single(list);

        var forbidden = await member.PostAsJsonAsync("/api/resources", Room("A102"), Json.Options);
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
    }

    [Fact]
    public async Task Sans_token_tout_est_refuse()
    {
        var anonymous = _factory.CreateClient();

        var response = await anonymous.GetAsync("/api/resources");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Les_attributs_json_font_l_aller_retour_intacts()
    {
        var attributes = JsonSerializer.Deserialize<JsonElement>("""{"building":"B","floor":2,"projector":true}""");
        var request = new CreateResourceRequest("B201", "room", 40, attributes);

        var created = await _adminA.PostAsJsonAsync("/api/resources", request, Json.Options);
        var dto = (await created.Content.ReadFromJsonAsync<ResourceDto>(Json.Options))!;

        var fetched = (await _adminA.GetFromJsonAsync<ResourceDto>($"/api/resources/{dto.Id}", Json.Options))!;

        Assert.NotNull(fetched.Attributes);
        Assert.Equal("B", fetched.Attributes.Value.GetProperty("building").GetString());
        Assert.Equal(2, fetched.Attributes.Value.GetProperty("floor").GetInt32());
        Assert.True(fetched.Attributes.Value.GetProperty("projector").GetBoolean());
    }

    [Fact]
    public async Task Update_modifie_tous_les_champs_et_delete_supprime()
    {
        var created = await _adminA.PostAsJsonAsync("/api/resources", Room("A101"), Json.Options);
        var dto = (await created.Content.ReadFromJsonAsync<ResourceDto>(Json.Options))!;

        var update = new UpdateResourceRequest("A101 bis", "lab", 12, ResourceStatus.Inactive, null);
        var updated = await _adminA.PutAsJsonAsync($"/api/resources/{dto.Id}", update, Json.Options);
        Assert.Equal(HttpStatusCode.NoContent, updated.StatusCode);

        var fetched = (await _adminA.GetFromJsonAsync<ResourceDto>($"/api/resources/{dto.Id}", Json.Options))!;
        Assert.Equal("A101 bis", fetched.Name);
        Assert.Equal("lab", fetched.Kind);
        Assert.Equal(12, fetched.Capacity);
        Assert.Equal(ResourceStatus.Inactive, fetched.Status);
        Assert.True(fetched.UpdatedAt > fetched.CreatedAt);

        var deleted = await _adminA.DeleteAsync($"/api/resources/{dto.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);

        var gone = await _adminA.GetAsync($"/api/resources/{dto.Id}");
        Assert.Equal(HttpStatusCode.NotFound, gone.StatusCode);
    }

    [Fact]
    public async Task Le_filtre_par_kind_fonctionne()
    {
        await _adminA.PostAsJsonAsync("/api/resources", Room("A101"), Json.Options);
        await _adminA.PostAsJsonAsync("/api/resources", new CreateResourceRequest("Mme Dupont", "teacher", null, null), Json.Options);

        var rooms = (await _adminA.GetFromJsonAsync<List<ResourceDto>>("/api/resources?kind=room", Json.Options))!;
        var teachers = (await _adminA.GetFromJsonAsync<List<ResourceDto>>("/api/resources?kind=teacher", Json.Options))!;

        Assert.Single(rooms);
        Assert.Single(teachers);
        Assert.Null(teachers[0].Capacity);
    }

    [Fact]
    public async Task Un_doublon_kind_plus_nom_renvoie_409()
    {
        await _adminA.PostAsJsonAsync("/api/resources", Room("A101"), Json.Options);

        var duplicate = await _adminA.PostAsJsonAsync("/api/resources", Room("A101"), Json.Options);

        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
    }
}
