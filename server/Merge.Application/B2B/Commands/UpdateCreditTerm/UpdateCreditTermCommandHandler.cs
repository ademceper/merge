using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Commands.UpdateCreditTerm;

public class UpdateCreditTermCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<UpdateCreditTermCommandHandler> logger) : IRequestHandler<UpdateCreditTermCommand, bool>
{

    public async Task<bool> Handle(UpdateCreditTermCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating credit term. CreditTermId: {CreditTermId}", request.Id);

        var creditTerm = await context.Set<CreditTerm>()
            .FirstOrDefaultAsync(ct => ct.Id == request.Id, cancellationToken);

        if (creditTerm == null)
        {
            logger.LogWarning("Credit term not found with Id: {CreditTermId}", request.Id);
            return false;
        }

        creditTerm.UpdateDetails(request.Dto.Name, request.Dto.PaymentDays, request.Dto.Terms);
        creditTerm.UpdateCreditLimit(request.Dto.CreditLimit);
        // creditTerm.UpdatedAt = DateTime.UtcNow; // Handled by entity
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Credit term updated successfully. CreditTermId: {CreditTermId}", request.Id);
        return true;
    }
}

