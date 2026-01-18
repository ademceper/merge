using MediatR;
using Merge.Domain.SharedKernel;

namespace Merge.Application.Governance.Commands.CreateAuditLog;

public record CreateAuditLogCommand(
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
    string Severity,
    string Module,
    bool IsSuccessful,
    string? ErrorMessage,
    string? AdditionalData,
    string IpAddress, // Controller'dan set edilecek
    string UserAgent // Controller'dan set edilecek
) : IRequest;
