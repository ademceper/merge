namespace Merge.Application.DTOs.Support;

/// <summary>
/// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
/// </summary>
public record TicketTrendDto(
    DateTime Date,
    int Opened,
    int Resolved,
    int Closed
);
