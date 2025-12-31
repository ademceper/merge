namespace Merge.Application.DTOs.Support;

public class TicketTrendDto
{
    public DateTime Date { get; set; }
    public int Opened { get; set; }
    public int Resolved { get; set; }
    public int Closed { get; set; }
}
