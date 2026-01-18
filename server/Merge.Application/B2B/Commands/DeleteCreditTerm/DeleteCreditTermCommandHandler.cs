using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Commands.DeleteCreditTerm;

public class DeleteCreditTermCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<DeleteCreditTermCommandHandler> logger) : IRequestHandler<DeleteCreditTermCommand, bool>
{

    public async Task<bool> Handle(DeleteCreditTermCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting credit term. CreditTermId: {CreditTermId}", request.Id);

        var creditTerm = await context.Set<CreditTerm>()
            .FirstOrDefaultAsync(ct => ct.Id == request.Id, cancellationToken);

        if (creditTerm is null)
        {
            logger.LogWarning("Credit term not found with Id: {CreditTermId}", request.Id);
            return false;
        }

        creditTerm.Delete();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Credit term deleted successfully. CreditTermId: {CreditTermId}", request.Id);
        return true;
    }
}

