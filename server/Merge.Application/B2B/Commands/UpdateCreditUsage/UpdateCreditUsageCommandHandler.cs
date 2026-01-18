using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Commands.UpdateCreditUsage;

public class UpdateCreditUsageCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<UpdateCreditUsageCommandHandler> logger) : IRequestHandler<UpdateCreditUsageCommand, bool>
{

    public async Task<bool> Handle(UpdateCreditUsageCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating credit usage. CreditTermId: {CreditTermId}, Amount: {Amount}",
            request.CreditTermId, request.Amount);

        var creditTerm = await context.Set<CreditTerm>()
            .FirstOrDefaultAsync(ct => ct.Id == request.CreditTermId, cancellationToken);

        if (creditTerm is null)
        {
            logger.LogWarning("Credit term not found with Id: {CreditTermId}", request.CreditTermId);
            return false;
        }

        creditTerm.UseCredit(request.Amount);
        // creditTerm.UpdatedAt = DateTime.UtcNow; // Handled by entity
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Credit usage updated successfully. CreditTermId: {CreditTermId}", request.CreditTermId);
        return true;
    }
}

