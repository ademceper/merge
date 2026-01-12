using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.DTOs.Cart;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using AutoMapper;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Commands.CreatePreOrderCampaign;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
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

