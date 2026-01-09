using MediatR;

namespace Merge.Application.Governance.Commands.DeleteOldAuditLogs;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteOldAuditLogsCommand(
    int DaysToKeep = 365
) : IRequest;

