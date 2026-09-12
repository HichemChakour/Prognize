using System.ComponentModel.DataAnnotations;
using Prognize.Api.Domain.Enums;

namespace Prognize.Api.Features.Auth;

public record RegisterRequest(
    [Required, MaxLength(200)] string OrganizationName,
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password,
    [Required, MaxLength(150)] string DisplayName);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public record AuthResponse(string Token, DateTimeOffset ExpiresAt, CurrentUserDto User);

public record CurrentUserDto(
    Guid Id,
    string Email,
    string DisplayName,
    UserRole Role,
    Guid OrganizationId,
    string OrganizationName);
