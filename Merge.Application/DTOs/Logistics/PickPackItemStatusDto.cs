namespace Merge.Application.DTOs.Logistics;

public class PickPackItemStatusDto
{
    public bool IsPicked { get; set; }
    public bool IsPacked { get; set; }
    public string? Location { get; set; }
}
