using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Prognize.Api.Data;
using Prognize.Api.Domain;
using Prognize.Api.Domain.Enums;
using Prognize.Api.Features.Auth;

namespace Prognize.Tests.Support;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public TestDatabase Db { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:Default", Db.ConnectionString);
    }

    public async Task<HttpClient> RegisterAdminAsync(string organizationName, string email)
    {
        var anonymous = CreateClient();
        var response = await anonymous.PostAsJsonAsync("/api/auth/register", new RegisterRequest(
            organizationName, email, "password1", "Admin"), Json.Options);
        response.EnsureSuccessStatusCode();

        var auth = (await response.Content.ReadFromJsonAsync<AuthResponse>(Json.Options))!;
        return ClientWithToken(auth.Token);
    }

    public async Task<HttpClient> CreateMemberClientAsync(Guid organizationId, string email)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokens = scope.ServiceProvider.GetRequiredService<TokenService>();

        var member = new AppUser
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            UserName = email,
            Email = email,
            DisplayName = "Member",
            Role = UserRole.Member,
        };
        db.Users.Add(member);
        await db.SaveChangesAsync();

        return ClientWithToken(tokens.CreateToken(member).Token);
    }

    private HttpClient ClientWithToken(string token)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await Db.DisposeAsync();
    }
}
