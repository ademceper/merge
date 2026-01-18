using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.DTOs.Seller;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Commands.RequestPayout;

public class RequestPayoutCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, IOptions<SellerSettings> sellerSettings, ILogger<RequestPayoutCommandHandler> logger) : IRequestHandler<RequestPayoutCommand, CommissionPayoutDto>
{
    private readonly SellerSettings sellerConfig = sellerSettings.Value;

    public async Task<CommissionPayoutDto> Handle(RequestPayoutCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Requesting payout. SellerId: {SellerId}, CommissionCount: {CommissionCount}",
            request.SellerId, request.CommissionIds.Count);

        var commissions = await context.Set<SellerCommission>()
            .Where(sc => request.CommissionIds.Contains(sc.Id) && sc.SellerId == request.SellerId)
            .Where(sc => sc.Status == CommissionStatus.Approved)
            .ToListAsync(cancellationToken);

        if (commissions.Count == 0)
        {
            logger.LogWarning("No approved commissions found for payout. SellerId: {SellerId}", request.SellerId);
            throw new BusinessException("Onaylanmış komisyon bulunamadı.");
        }

        var settings = await context.Set<SellerCommissionSettings>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SellerId == request.SellerId, cancellationToken);

        var totalAmount = await context.Set<SellerCommission>()
            .AsNoTracking()
            .Where(sc => request.CommissionIds.Contains(sc.Id) && sc.SellerId == request.SellerId && sc.Status == CommissionStatus.Approved)
            .SumAsync(c => c.NetAmount, cancellationToken);

        if (settings != null && totalAmount < settings.MinimumPayoutAmount)
        {
            logger.LogWarning("Payout amount below minimum. SellerId: {SellerId}, Amount: {Amount}, Minimum: {Minimum}",
                request.SellerId, totalAmount, settings.MinimumPayoutAmount);
            throw new ValidationException($"Minimum ödeme tutarı {settings.MinimumPayoutAmount}.");
        }

        var transactionFee = totalAmount * (sellerConfig.PayoutTransactionFeeRate / 100);
        var netAmount = totalAmount - transactionFee;

        var payoutNumber = await GeneratePayoutNumberAsync(cancellationToken);

        var payout = CommissionPayout.Create(
            sellerId: request.SellerId,
            payoutNumber: payoutNumber,
            totalAmount: totalAmount,
            transactionFee: transactionFee,
            paymentMethod: settings?.PaymentMethod ?? sellerConfig.DefaultPaymentMethod,
            paymentDetails: settings?.PaymentDetails);

        await context.Set<CommissionPayout>().AddAsync(payout, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var commission in commissions)
        {
            payout.AddItem(commission.Id);

            commission.MarkAsPaid(payoutNumber);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        payout = await context.Set<CommissionPayout>()
            .AsNoTracking()
            .Include(p => p.Seller)
            .Include(p => p.Items)
                .ThenInclude(i => i.Commission)
                    .ThenInclude(c => c.Order)
            .FirstOrDefaultAsync(p => p.Id == payout.Id, cancellationToken);

        logger.LogInformation("Payout requested. PayoutId: {PayoutId}, PayoutNumber: {PayoutNumber}, Amount: {Amount}",
            payout!.Id, payoutNumber, netAmount);

        return mapper.Map<CommissionPayoutDto>(payout);
    }

    private async Task<string> GeneratePayoutNumberAsync(CancellationToken cancellationToken)
    {
        var lastPayout = await context.Set<CommissionPayout>()
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        int nextNumber = 1;
        if (lastPayout != null && lastPayout.PayoutNumber.StartsWith("PAY-"))
        {
            var numberPart = lastPayout.PayoutNumber.Substring(4);
            if (int.TryParse(numberPart, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"PAY-{nextNumber:D6}";
    }
}
