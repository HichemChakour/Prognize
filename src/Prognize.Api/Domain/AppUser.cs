using Microsoft.AspNetCore.Identity;
using Prognize.Api.Common.Tenancy;
using Prognize.Api.Domain.Enums;

namespace Prognize.Api.Domain;


public class AppUser : IdentityUser<Guid>, ITenantOwned
{
    public Guid OrganizationId { get; set; }

    public Organization? Organization { get; set; }

    public required string DisplayName { get; set; }

    public UserRole Role { get; set; } = UserRole.Member;

    public DateTimeOffset CreatedAt { get; set; }
}
