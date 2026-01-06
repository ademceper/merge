using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Cart;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using AutoMapper;

namespace Merge.Application.Cart.Commands.CreatePreOrder;

public class CreatePreOrderCommandHandler : IRequestHandler<CreatePreOrderCommand, PreOrderDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreatePreOrderCommandHandler> _logger;

    public CreatePreOrderCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreatePreOrderCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PreOrderDto> Handle(CreatePreOrderCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var product = await _context.Set<Merge.Domain.Entities.Product>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product == null)
            {
                throw new NotFoundException("Ürün", request.ProductId);
            }

            var campaign = await _context.Set<PreOrderCampaign>()
                .AsNoTracking()
                .Where(c => c.ProductId == request.ProductId && c.IsActive)
                .Where(c => c.StartDate <= DateTime.UtcNow && c.EndDate >= DateTime.UtcNow)
                .FirstOrDefaultAsync(cancellationToken);

            if (campaign == null)
            {
                throw new BusinessException("Bu ürün için aktif ön sipariş kampanyası yok.");
            }

            if (campaign.MaxQuantity > 0 && campaign.CurrentQuantity >= campaign.MaxQuantity)
            {
                throw new BusinessException("Ön sipariş kampanyası dolu.");
            }

            var price = campaign.SpecialPrice > 0 ? campaign.SpecialPrice : product.Price;
            var depositAmount = price * (campaign.DepositPercentage / 100);

            var preOrder = PreOrder.Create(
                request.UserId,
                request.ProductId,
                request.Quantity,
                price,
                depositAmount,
                campaign.ExpectedDeliveryDate,
                campaign.EndDate,
                request.Notes,
                request.VariantOptions);

            if (depositAmount == 0)
            {
                preOrder.Confirm();
            }

            await _context.Set<PreOrder>().AddAsync(preOrder, cancellationToken);

            var campaignToUpdate = await _context.Set<PreOrderCampaign>()
                .FirstOrDefaultAsync(c => c.Id == campaign.Id, cancellationToken);

            if (campaignToUpdate == null)
            {
                throw new NotFoundException("Kampanya", campaign.Id);
            }

            campaignToUpdate.IncrementQuantity(request.Quantity);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            preOrder = await _context.Set<PreOrder>()
                .AsNoTracking()
                .Include(po => po.Product)
                .FirstOrDefaultAsync(po => po.Id == preOrder.Id, cancellationToken);

            return _mapper.Map<PreOrderDto>(preOrder!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "PreOrder olusturma hatasi. UserId: {UserId}, ProductId: {ProductId}",
                request.UserId, request.ProductId);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

