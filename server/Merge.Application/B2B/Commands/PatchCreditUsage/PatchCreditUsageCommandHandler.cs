using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.B2B;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Commands.PatchCreditUsage;

public class PatchCreditUsageCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMediator mediator,
    ILogger<PatchCreditUsageCommandHandler> logger) : IRequestHandler<PatchCreditUsageCommand, bool>
{
    public async Task<bool> Handle(PatchCreditUsageCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Patching credit usage. CreditTermId: {CreditTermId}", request.CreditTermId);

        if (!request.PatchDto.Amount.HasValue)
        {
            logger.LogWarning("No amount provided for credit usage patch");
            return false;
        }

        var updateCommand = new UpdateCreditUsageCommand(request.CreditTermId, request.PatchDto.Amount.Value);

        return await mediator.Send(updateCommand, cancellationToken);
    }
}
