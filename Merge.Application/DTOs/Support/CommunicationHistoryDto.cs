namespace Merge.Application.DTOs.Support;

public class CommunicationHistoryDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int TotalCommunications { get; set; }
    public Dictionary<string, int> CommunicationsByType { get; set; } = new();
    public Dictionary<string, int> CommunicationsByChannel { get; set; } = new();
    public List<CustomerCommunicationDto> RecentCommunications { get; set; } = new();
    public DateTime? LastCommunicationDate { get; set; }
}
