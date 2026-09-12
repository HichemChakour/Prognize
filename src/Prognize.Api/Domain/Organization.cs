namespace Prognize.Api.Domain;

public class Organization
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Slug { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
}
