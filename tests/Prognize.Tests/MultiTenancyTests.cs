using Microsoft.EntityFrameworkCore;
using Prognize.Api.Common.Tenancy;
using Prognize.Api.Data;
using Prognize.Api.Domain;
using Prognize.Api.Domain.Enums;

namespace Prognize.Tests;

/// <summary>
/// LE test de la Phase 0.
///
/// Le multi-tenant repose sur deux mecanismes invisibles (un filtre applique
/// automatiquement en lecture, un estampillage automatique en ecriture). Du
/// code invisible qui n'est pas teste est du code dont on ne sait pas s'il
/// marche — et ici, s'il ne marche pas, un client voit les donnees d'un autre.
/// C'est le seul bug vraiment inacceptable du projet.
///
/// On teste contre un VRAI Postgres (base jetable creee puis supprimee) et non
/// contre le provider InMemory : InMemory ne genere pas de SQL, il ne prouverait
/// donc pas que le filtre finit bien en "WHERE organization_id = ...".
///
/// Prerequis : "docker compose up -d".
///
/// IAsyncLifetime (xUnit) = le setUp/tearDown de PHPUnit, en version async.
/// </summary>
public class MultiTenancyTests : IAsyncLifetime
{
    // Nom unique par execution : deux lancements en parallele ne se marchent
    // pas dessus, et un test qui plante ne pollue pas le suivant.
    private readonly string _dbName = $"prognize_test_{Guid.NewGuid():N}";
    private string ConnectionString =>
        $"Host=localhost;Port=5432;Database={_dbName};Username=prognize;Password=prognize_dev_pwd";

    private Guid _orgA;
    private Guid _orgB;

    /// <summary>
    /// Fabrique un DbContext pour un tenant donne.
    /// C'est ici que l'interface ITenantContext montre son interet : en prod
    /// c'est HttpTenantContext (qui lit le JWT), en test c'est FixedTenantContext.
    /// Le DbContext, lui, ne voit aucune difference.
    /// </summary>
    private AppDbContext CreateContext(Guid? tenantId)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new AppDbContext(options, new FixedTenantContext(tenantId));
    }

    public async Task InitializeAsync()
    {
        // tenant = null : aucun filtre ne s'applique et aucun estampillage
        // n'a lieu. C'est le mode "administrateur" dont on a besoin pour
        // fabriquer le jeu d'essai des DEUX organisations.
        await using var db = CreateContext(null);
        await db.Database.MigrateAsync();

        var a = new Organization { Name = "Lycee A", Slug = "lycee-a" };
        var b = new Organization { Name = "Lycee B", Slug = "lycee-b" };
        db.Organizations.AddRange(a, b);

        db.Users.AddRange(
            new AppUser
            {
                Id = Guid.NewGuid(),
                OrganizationId = a.Id,       // renseigne a la main : pas de tenant courant
                DisplayName = "Alice (A)",
                UserName = "alice@a.test",
                Email = "alice@a.test",
                Role = UserRole.Admin
            },
            new AppUser
            {
                Id = Guid.NewGuid(),
                OrganizationId = b.Id,
                DisplayName = "Bob (B)",
                UserName = "bob@b.test",
                Email = "bob@b.test",
                Role = UserRole.Member
            });

        await db.SaveChangesAsync();

        _orgA = a.Id;
        _orgB = b.Id;
    }

    // Teardown : on supprime la base jetable.
    public async Task DisposeAsync()
    {
        await using var db = CreateContext(null);
        await db.Database.EnsureDeletedAsync();
    }

    // =================================================================
    // VERROU 1 : LA LECTURE (global query filters)
    // =================================================================

    [Fact]
    public async Task Un_tenant_ne_voit_que_ses_propres_utilisateurs()
    {
        await using var db = CreateContext(_orgA);

        // Aucun "where" ecrit a la main : le filtre est ajoute par EF.
        var users = await db.Users.ToListAsync();

        Assert.Single(users);
        Assert.Equal("Alice (A)", users[0].DisplayName);
    }

    [Fact]
    public async Task Un_tenant_ne_voit_que_sa_propre_organisation()
    {
        await using var db = CreateContext(_orgB);

        var orgs = await db.Organizations.ToListAsync();

        Assert.Single(orgs);
        Assert.Equal("lycee-b", orgs[0].Slug);
    }

    [Fact]
    public async Task Acceder_par_id_a_la_ressource_d_un_autre_tenant_renvoie_null()
    {
        // Le scenario d'attaque reel : un utilisateur du tenant A devine l'id
        // d'une ressource du tenant B et l'appelle dans l'URL.
        await using var db = CreateContext(_orgA);

        var stolen = await db.Organizations.FirstOrDefaultAsync(o => o.Id == _orgB);

        // null => le controller renverra 404. L'attaquant n'apprend meme pas
        // que la ressource existe.
        Assert.Null(stolen);
    }

    [Fact]
    public async Task Sans_tenant_aucune_donnee_n_est_visible()
    {
        // Le cas "echouer du bon cote" : token absent ou claim illisible.
        // On veut 0 ligne, surtout pas toutes les lignes.
        await using var db = CreateContext(null);

        Assert.Empty(await db.Users.ToListAsync());
        Assert.Empty(await db.Organizations.ToListAsync());
    }

    [Fact]
    public async Task IgnoreQueryFilters_permet_de_contourner_volontairement()
    {
        // La porte de sortie explicite, dont on aura besoin au login (il faut
        // retrouver l'utilisateur AVANT de connaitre son organisation).
        // Elle doit etre un geste conscient, jamais un effet de bord.
        await using var db = CreateContext(_orgA);

        var all = await db.Users.IgnoreQueryFilters().ToListAsync();

        Assert.Equal(2, all.Count);
    }

    // =================================================================
    // VERROU 2 : L'ECRITURE (override de SaveChanges)
    // =================================================================

    [Fact]
    public async Task Une_insertion_est_estampillee_avec_le_tenant_courant()
    {
        await using (var db = CreateContext(_orgA))
        {
            // Noter ce qui N'EST PAS ecrit : aucun OrganizationId.
            // C'est tout l'interet — un controller ne peut pas se tromper de
            // tenant, et un client ne peut pas en imposer un via son JSON.
            db.Users.Add(new AppUser
            {
                Id = Guid.NewGuid(),
                DisplayName = "Charlie (A)",
                UserName = "charlie@a.test",
                Email = "charlie@a.test",
                Role = UserRole.Member
            });
            await db.SaveChangesAsync();
        }

        await using var verify = CreateContext(_orgA);
        var charlie = await verify.Users.SingleAsync(u => u.DisplayName == "Charlie (A)");

        Assert.Equal(_orgA, charlie.OrganizationId);
    }

    [Fact]
    public async Task Deplacer_une_ligne_vers_un_autre_tenant_est_impossible()
    {
        // Sans le case EntityState.Modified dans ApplyTenantRules, ce test
        // echouerait : les query filters ne protegent QUE la lecture.
        await using (var db = CreateContext(_orgA))
        {
            var alice = await db.Users.SingleAsync();
            alice.OrganizationId = _orgB;       // tentative de transfert
            await db.SaveChangesAsync();        // doit etre ignoree silencieusement
        }

        await using var verify = CreateContext(_orgA);

        // Alice est toujours chez A : la colonne a ete exclue du UPDATE.
        Assert.Single(await verify.Users.ToListAsync());
    }
}
