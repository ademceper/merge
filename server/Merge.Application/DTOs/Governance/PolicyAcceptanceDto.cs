namespace Merge.Application.DTOs.Governance;

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
