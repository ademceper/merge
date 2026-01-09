using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Governance.Commands.CreateAuditLog;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CreateAuditLogCommandHandler : IRequestHandler<CreateAuditLogCommand>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateAuditLogCommandHandler> _logger;

    public CreateAuditLogCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<CreateAuditLogCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(CreateAuditLogCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating audit log. Action: {Action}, EntityType: {EntityType}, UserId: {UserId}",
            request.Action, request.EntityType, request.UserId);

        try
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
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

            await _context.Set<AuditLog>().AddAsync(audit, cancellationToken);
            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Audit log created. AuditLogId: {AuditLogId}, Action: {Action}",
                audit.Id, request.Action);
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex, "Error creating audit log. Action: {Action}, EntityType: {EntityType}",
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

