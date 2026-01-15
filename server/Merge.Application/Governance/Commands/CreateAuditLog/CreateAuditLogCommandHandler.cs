using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using Merge.Domain.SharedKernel;

namespace Merge.Application.Governance.Commands.CreateAuditLog;

public class CreateAuditLogCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<CreateAuditLogCommandHandler> logger) : IRequestHandler<CreateAuditLogCommand>
{

    public async Task Handle(CreateAuditLogCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating audit log. Action: {Action}, EntityType: {EntityType}, UserId: {UserId}",
            request.Action, request.EntityType, request.UserId);

        try
        {
            var audit = AuditLog.Create(
                action: request.Action,
                entityType: request.EntityType,
                tableName: request.TableName,
                ipAddress: request.IpAddress,
                userAgent: request.UserAgent,
                module: request.Module,
                severity: ParseSeverity(request.Severity),
                userId: request.UserId,
                userEmail: request.UserEmail,
                entityId: request.EntityId,
                primaryKey: request.PrimaryKey,
                oldValues: request.OldValues,
                newValues: request.NewValues,
                changes: request.Changes,
                additionalData: request.AdditionalData,
                isSuccessful: request.IsSuccessful,
                errorMessage: request.ErrorMessage);

            await context.Set<AuditLog>().AddAsync(audit, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Audit log created. AuditLogId: {AuditLogId}, Action: {Action}",
                audit.Id, request.Action);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating audit log. Action: {Action}, EntityType: {EntityType}",
                request.Action, request.EntityType);
            throw;
        }
    }

    private AuditSeverity ParseSeverity(string severity)
    {
        return severity.ToLower() switch
        {
            "warning" => AuditSeverity.Warning,
            "error" => AuditSeverity.Error,
            "critical" => AuditSeverity.Critical,
            _ => AuditSeverity.Info
        };
    }
}
