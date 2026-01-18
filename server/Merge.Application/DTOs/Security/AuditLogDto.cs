using Merge.Domain.SharedKernel;
namespace Merge.Application.DTOs.Security;

public record AuditLogDto(
    Guid Id,
    Guid? UserId,
    string UserEmail,
    string Action,
    string EntityType,
    Guid? EntityId,
    string TableName,
    string PrimaryKey,
    string OldValues,
    string NewValues,
    string Changes,
    string IpAddress,
    string UserAgent,
    string Severity,
    string Module,
    bool IsSuccessful,
    string? ErrorMessage,
    DateTime CreatedAt);