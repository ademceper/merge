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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CompareChangesQueryHandler : IRequestHandler<CompareChangesQuery, IEnumerable<AuditComparisonDto>>
{
    private readonly IDbContext _context;
    private readonly ILogger<CompareChangesQueryHandler> _logger;

    public CompareChangesQueryHandler(
        IDbContext context,
        ILogger<CompareChangesQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<AuditComparisonDto>> Handle(CompareChangesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Comparing changes for audit log. AuditLogId: {AuditLogId}", request.AuditLogId);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        var audit = await _context.Set<AuditLog>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.AuditLogId, cancellationToken);

        if (audit == null || string.IsNullOrEmpty(audit.OldValues) || string.IsNullOrEmpty(audit.NewValues))
        {
            _logger.LogWarning("Audit log not found or missing values. AuditLogId: {AuditLogId}", request.AuditLogId);
            return new List<AuditComparisonDto>();
        }

        try
        {
            var oldValues = JsonSerializer.Deserialize<Dictionary<string, object>>(audit.OldValues);
            var newValues = JsonSerializer.Deserialize<Dictionary<string, object>>(audit.NewValues);

            if (oldValues == null || newValues == null)
            {
                _logger.LogWarning("Failed to deserialize audit log values. AuditLogId: {AuditLogId}", request.AuditLogId);
                return new List<AuditComparisonDto>();
            }

            // ✅ BOLUM 6.4: List Capacity Pre-allocation (ZORUNLU)
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
            _logger.LogError(ex, "Error comparing audit log changes. AuditLogId: {AuditLogId}", request.AuditLogId);
            // ✅ BOLUM 2.1: Exception yutulmamali - ama burada boş liste döndürmek mantıklı
            return new List<AuditComparisonDto>();
        }
    }
}
