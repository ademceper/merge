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

namespace Merge.Application.B2B.Commands.PatchCreditTerm;

public class PatchCreditTermCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMediator mediator,
    ILogger<PatchCreditTermCommandHandler> logger) : IRequestHandler<PatchCreditTermCommand, bool>
{
    public async Task<bool> Handle(PatchCreditTermCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Patching credit term. CreditTermId: {CreditTermId}", request.Id);

        var creditTerm = await context.Set<CreditTerm>()
            .FirstOrDefaultAsync(ct => ct.Id == request.Id, cancellationToken);

        if (creditTerm == null)
        {
            logger.LogWarning("Credit term not found. CreditTermId: {CreditTermId}", request.Id);
            return false;
        }

        var dto = new CreateCreditTermDto
        {
            OrganizationId = request.PatchDto.OrganizationId ?? creditTerm.OrganizationId,
            Name = request.PatchDto.Name ?? creditTerm.Name,
            PaymentDays = request.PatchDto.PaymentDays ?? creditTerm.PaymentDays,
            CreditLimit = request.PatchDto.CreditLimit ?? creditTerm.CreditLimit,
            Terms = request.PatchDto.Terms ?? creditTerm.Terms
        };

        var updateCommand = new UpdateCreditTermCommand(request.Id, dto);

        return await mediator.Send(updateCommand, cancellationToken);
    }
}
