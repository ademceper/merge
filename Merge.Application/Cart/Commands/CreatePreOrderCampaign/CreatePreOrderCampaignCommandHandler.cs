using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.DTOs.Cart;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using AutoMapper;

namespace Merge.Application.Cart.Commands.CreatePreOrderCampaign;

public class CreatePreOrderCampaignCommandHandler : IRequestHandler<CreatePreOrderCampaignCommand, PreOrderCampaignDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreatePreOrderCampaignCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PreOrderCampaignDto> Handle(CreatePreOrderCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = PreOrderCampaign.Create(
            request.Name,
            request.Description,
            request.ProductId,
            request.StartDate,
            request.EndDate,
            request.ExpectedDeliveryDate,
            request.MaxQuantity,
            request.DepositPercentage,
            request.SpecialPrice,
            request.NotifyOnAvailable);

        await _context.Set<PreOrderCampaign>().AddAsync(campaign, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        campaign = await _context.Set<PreOrderCampaign>()
            .AsNoTracking()
            .Include(c => c.Product)
            .FirstOrDefaultAsync(c => c.Id == campaign.Id, cancellationToken);

        return _mapper.Map<PreOrderCampaignDto>(campaign!);
    }
}

