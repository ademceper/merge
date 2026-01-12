namespace Merge.Application.DTOs.Support;

/// <summary>
/// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
/// </summary>
public record CategoryTicketCountDto(
    string Category,
    int Count,
    decimal Percentage
);
