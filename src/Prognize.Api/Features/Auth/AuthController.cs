using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prognize.Api.Common;
using Prognize.Api.Data;
using Prognize.Api.Domain;
using Prognize.Api.Domain.Enums;

namespace Prognize.Api.Features.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;
    private readonly TokenService _tokenService;

    public AuthController(AppDbContext db, UserManager<AppUser> userManager, TokenService tokenService)
    {
        _db = db;
        _userManager = userManager;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        var normalizedEmail = _userManager.NormalizeEmail(request.Email);

        var emailTaken = await _db.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.NormalizedEmail == normalizedEmail, ct);

        if (emailTaken)
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Cet email est déjà utilisé.");

        var slug = await GenerateUniqueSlugAsync(request.OrganizationName, ct);

        await using var transaction = await _db.Database.BeginTransactionAsync(ct);

        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            Name = request.OrganizationName.Trim(),
            Slug = slug,
        };
        _db.Organizations.Add(organization);
        await _db.SaveChangesAsync(ct);

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            OrganizationId = organization.Id,
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName.Trim(),
            Role = UserRole.Admin,
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            await transaction.RollbackAsync(ct);
            foreach (var error in result.Errors)
                ModelState.AddModelError(error.Code, error.Description);
            return ValidationProblem(ModelState);
        }

        await transaction.CommitAsync(ct);

        var (token, expiresAt) = _tokenService.CreateToken(user);
        var response = new AuthResponse(token, expiresAt, ToDto(user, organization));
        return CreatedAtAction(nameof(Me), null, response);
    }

    [HttpPost("login")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var normalizedEmail = _userManager.NormalizeEmail(request.Email);

        var user = await _db.Users
            .IgnoreQueryFilters()
            .Include(u => u.Organization)
            .SingleOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, ct);

        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Email ou mot de passe incorrect.");

        var (token, expiresAt) = _tokenService.CreateToken(user);
        return Ok(new AuthResponse(token, expiresAt, ToDto(user, user.Organization!)));
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType<CurrentUserDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CurrentUserDto>> Me(CancellationToken ct)
    {
        if (!Guid.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out var userId))
            return Unauthorized();

        var user = await _db.Users
            .Include(u => u.Organization)
            .SingleOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null)
            return Unauthorized();

        return Ok(ToDto(user, user.Organization!));
    }

    private async Task<string> GenerateUniqueSlugAsync(string organizationName, CancellationToken ct)
    {
        var baseSlug = Slugify.From(organizationName);
        var slug = baseSlug;
        var suffix = 2;

        while (await _db.Organizations.IgnoreQueryFilters().AnyAsync(o => o.Slug == slug, ct))
            slug = $"{baseSlug}-{suffix++}";

        return slug;
    }

    private static CurrentUserDto ToDto(AppUser user, Organization organization) =>
        new(user.Id, user.Email!, user.DisplayName, user.Role, organization.Id, organization.Name);
}
