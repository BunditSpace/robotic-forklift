namespace Forklift.Core.Entities;

/// <summary>
/// Represents a forklift entity.
/// </summary>
public class ForkLift
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ModelNumber { get; set; } = string.Empty;
    public DateTime ManufacturingDate { get; set; }
}
