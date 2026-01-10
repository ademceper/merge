using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.ML.Commands.DeleteFraudDetectionRule;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class DeleteFraudDetectionRuleCommandHandler : IRequestHandler<DeleteFraudDetectionRuleCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteFraudDetectionRuleCommandHandler> _logger;

    public DeleteFraudDetectionRuleCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeleteFraudDetectionRuleCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteFraudDetectionRuleCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Deleting fraud detection rule. RuleId: {RuleId}", request.Id);

        // ✅ PERFORMANCE: Removed manual !r.IsDeleted (Global Query Filter)
        var rule = await _context.Set<FraudDetectionRule>()
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (rule == null)
        {
            _logger.LogWarning("Fraud detection rule not found. RuleId: {RuleId}", request.Id);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı (Soft Delete)
        rule.MarkAsDeleted();

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Fraud detection rule deleted. RuleId: {RuleId}", request.Id);
        return true;
    }
}
