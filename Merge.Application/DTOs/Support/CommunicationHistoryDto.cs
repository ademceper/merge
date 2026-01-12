namespace Merge.Application.DTOs.Support;

/// <summary>
/// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
/// </summary>
public record CommunicationHistoryDto(
    Guid UserId,
    string UserName,
    int TotalCommunications,
    IReadOnlyDictionary<string, int> CommunicationsByType,
    IReadOnlyDictionary<string, int> CommunicationsByChannel,
    IReadOnlyList<CustomerCommunicationDto> RecentCommunications,
    DateTime? LastCommunicationDate
);
