using MediatR;
using Merge.Domain.SharedKernel;

namespace Merge.Application.Governance.Commands.DeleteOldAuditLogs;

public record DeleteOldAuditLogsCommand(
    int DaysToKeep = 365
) : IRequest;
