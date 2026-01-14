using Merge.Domain.ValueObjects;
namespace Merge.Application.DTOs.Support;

/// <summary>
/// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
/// </summary>
public record PriorityTicketCountDto(
    string Priority,
    int Count,
    decimal Percentage
);
