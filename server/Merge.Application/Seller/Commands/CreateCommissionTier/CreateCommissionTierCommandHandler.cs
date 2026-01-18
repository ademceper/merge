using MediatR;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Seller;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Commands.CreateCommissionTier;

public class CreateCommissionTierCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateCommissionTierCommandHandler> logger) : IRequestHandler<CreateCommissionTierCommand, CommissionTierDto>
{

    public async Task<CommissionTierDto> Handle(CreateCommissionTierCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating commission tier. Name: {Name}, CommissionRate: {CommissionRate}%",
            request.Name, request.CommissionRate);

        var tier = CommissionTier.Create(
            name: request.Name,
            minSales: request.MinSales,
            maxSales: request.MaxSales,
            commissionRate: request.CommissionRate,
            platformFeeRate: request.PlatformFeeRate,
            priority: request.Priority);

        await context.Set<CommissionTier>().AddAsync(tier, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Commission tier created. TierId: {TierId}", tier.Id);

        return mapper.Map<CommissionTierDto>(tier);
    }
}
