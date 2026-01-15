using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Security;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using System.Text.Json;
using Merge.Domain.Interfaces;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using Merge.Domain.SharedKernel;

namespace Merge.Application.Governance.Queries.CompareChanges;

public class CompareChangesQueryHandler(
    IDbContext context,
    ILogger<CompareChangesQueryHandler> logger) : IRequestHandler<CompareChangesQuery, IEnumerable<AuditComparisonDto>>
{

    public async Task<IEnumerable<AuditComparisonDto>> Handle(CompareChangesQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Comparing changes for audit log. AuditLogId: {AuditLogId}", request.AuditLogId);

        var audit = await context.Set<AuditLog>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.AuditLogId, cancellationToken);

        if (audit == null || string.IsNullOrEmpty(audit.OldValues) || string.IsNullOrEmpty(audit.NewValues))
        {
            logger.LogWarning("Audit log not found or missing values. AuditLogId: {AuditLogId}", request.AuditLogId);
            return new List<AuditComparisonDto>();
        }

        try
        {
            var oldValues = JsonSerializer.Deserialize<Dictionary<string, object>>(audit.OldValues);
            var newValues = JsonSerializer.Deserialize<Dictionary<string, object>>(audit.NewValues);

            if (oldValues == null || newValues == null)
            {
                logger.LogWarning("Failed to deserialize audit log values. AuditLogId: {AuditLogId}", request.AuditLogId);
                return new List<AuditComparisonDto>();
            }

            var comparisons = new List<AuditComparisonDto>(newValues.Count);

            foreach (var key in newValues.Keys)
            {
                var oldValue = oldValues.ContainsKey(key) ? oldValues[key]?.ToString() ?? "" : "";
                var newValue = newValues[key]?.ToString() ?? "";

                if (oldValue != newValue)
                {
                    comparisons.Add(new AuditComparisonDto
                    {
                        FieldName = key,
                        OldValue = oldValue,
                        NewValue = newValue
                    });
                }
            }

            return comparisons;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error comparing audit log changes. AuditLogId: {AuditLogId}", request.AuditLogId);
            return new List<AuditComparisonDto>();
        }
    }
}
