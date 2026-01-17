using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.ML.Commands.ReviewFraudAlert;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class ReviewFraudAlertCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<ReviewFraudAlertCommandHandler> logger) : IRequestHandler<ReviewFraudAlertCommand, bool>
{

    public async Task<bool> Handle(ReviewFraudAlertCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Reviewing fraud alert. AlertId: {AlertId}, ReviewedByUserId: {ReviewedByUserId}, Status: {Status}",
            request.AlertId, request.ReviewedByUserId, request.Status);

        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        var alert = await context.Set<FraudAlert>()
            .FirstOrDefaultAsync(a => a.Id == request.AlertId, cancellationToken);

        if (alert == null)
        {
            logger.LogWarning("Fraud alert not found. AlertId: {AlertId}", request.AlertId);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
        // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
        if (Enum.TryParse<FraudAlertStatus>(request.Status, true, out var statusEnum))
        {
            alert.Review(request.ReviewedByUserId, statusEnum, request.Notes);
        }
        else
        {
            logger.LogWarning("Invalid status value. Status: {Status}", request.Status);
            return false;
        }

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Fraud alert reviewed. AlertId: {AlertId}", request.AlertId);
        return true;
    }
}
