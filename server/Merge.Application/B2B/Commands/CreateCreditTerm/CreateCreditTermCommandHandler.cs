using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.B2B;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using AutoMapper;
using OrganizationEntity = Merge.Domain.Modules.Identity.Organization;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Commands.CreateCreditTerm;

public class CreateCreditTermCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CreateCreditTermCommandHandler> logger) : IRequestHandler<CreateCreditTermCommand, CreditTermDto>
{

    public async Task<CreditTermDto> Handle(CreateCreditTermCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating credit term. OrganizationId: {OrganizationId}, Name: {Name}",
            request.Dto.OrganizationId, request.Dto.Name);

        var organization = await context.Set<OrganizationEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == request.Dto.OrganizationId, cancellationToken);

        if (organization is null)
        {
            throw new NotFoundException("Organizasyon", Guid.Empty);
        }

        var creditTerm = CreditTerm.Create(
            request.Dto.OrganizationId,
            organization,
            request.Dto.Name,
            request.Dto.PaymentDays,
            request.Dto.CreditLimit,
            request.Dto.Terms);

        await context.Set<CreditTerm>().AddAsync(creditTerm, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        creditTerm = await context.Set<CreditTerm>()
            .AsNoTracking()
            .Include(ct => ct.Organization)
            .FirstOrDefaultAsync(ct => ct.Id == creditTerm.Id, cancellationToken);

        logger.LogInformation("Credit term created successfully. CreditTermId: {CreditTermId}", creditTerm!.Id);

        return mapper.Map<CreditTermDto>(creditTerm);
    }
}

