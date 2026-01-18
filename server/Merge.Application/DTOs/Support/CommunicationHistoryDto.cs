namespace Merge.Application.DTOs.Support;


public record CommunicationHistoryDto(
    Guid UserId,
    string UserName,
    int TotalCommunications,
    IReadOnlyDictionary<string, int> CommunicationsByType,
    IReadOnlyDictionary<string, int> CommunicationsByChannel,
    IReadOnlyList<CustomerCommunicationDto> RecentCommunications,
    DateTime? LastCommunicationDate
);
