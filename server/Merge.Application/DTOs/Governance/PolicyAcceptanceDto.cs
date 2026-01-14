namespace Merge.Application.DTOs.Governance;

// ✅ BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
public record PolicyAcceptanceDto(
    Guid Id,
    Guid PolicyId,
    string PolicyTitle,
    Guid UserId,
    string UserName,
    string AcceptedVersion,
    string IpAddress,
    DateTime AcceptedAt,
    bool IsActive);
