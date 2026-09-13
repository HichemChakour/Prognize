namespace Prognize.Api.Domain;

public class ResourceRequirement
{
    public required string Kind { get; set; }
    public Guid? ResourceId { get; set; }
    public int? MinCapacity { get; set; }
}
