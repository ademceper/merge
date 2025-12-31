namespace Merge.Application.DTOs.Support;

public class PriorityTicketCountDto
{
    public string Priority { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}
