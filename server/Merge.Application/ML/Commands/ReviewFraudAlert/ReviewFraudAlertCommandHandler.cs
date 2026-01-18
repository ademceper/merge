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

public class ReviewFraudAlertCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<ReviewFraudAlertCommandHandler> logger) : IRequestHandler<ReviewFraudAlertCommand, bool>
{

    public async Task<bool> Handle(ReviewFraudAlertCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Reviewing fraud alert. AlertId: {AlertId}, ReviewedByUserId: {ReviewedByUserId}, Status: {Status}",
            request.AlertId, request.ReviewedByUserId, request.Status);

        var alert = await context.Set<FraudAlert>()
            .FirstOrDefaultAsync(a => a.Id == request.AlertId, cancellationToken);

        if (alert is null)
        {
            logger.LogWarning("Fraud alert not found. AlertId: {AlertId}", request.AlertId);
            return false;
        }

        if (Enum.TryParse<FraudAlertStatus>(request.Status, true, out var statusEnum))
        {
            alert.Review(request.ReviewedByUserId, statusEnum, request.Notes);
        }
        else
        {
            logger.LogWarning("Invalid status value. Status: {Status}", request.Status);
            return false;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Fraud alert reviewed. AlertId: {AlertId}", request.AlertId);
        return true;
    }
}
