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

namespace Merge.Application.Seller.Commands.RequestPayout;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class RequestPayoutCommandHandler : IRequestHandler<RequestPayoutCommand, CommissionPayoutDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IOptions<SellerSettings> _sellerSettings;
    private readonly ILogger<RequestPayoutCommandHandler> _logger;

    public RequestPayoutCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IOptions<SellerSettings> sellerSettings,
        ILogger<RequestPayoutCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _sellerSettings = sellerSettings;
        _logger = logger;
    }

    public async Task<CommissionPayoutDto> Handle(RequestPayoutCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Requesting payout. SellerId: {SellerId}, CommissionCount: {CommissionCount}",
            request.SellerId, request.CommissionIds.Count);

        // ✅ PERFORMANCE: Removed manual !sc.IsDeleted (Global Query Filter)
        // ✅ FIX: Tracking gerekli çünkü commission'ları update edeceğiz
        var commissions = await _context.Set<SellerCommission>()
            .Where(sc => request.CommissionIds.Contains(sc.Id) && sc.SellerId == request.SellerId)
            .Where(sc => sc.Status == CommissionStatus.Approved)
            .ToListAsync(cancellationToken);

        if (commissions.Count == 0)
        {
            _logger.LogWarning("No approved commissions found for payout. SellerId: {SellerId}", request.SellerId);
            throw new BusinessException("Onaylanmış komisyon bulunamadı.");
        }

        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var settings = await _context.Set<SellerCommissionSettings>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SellerId == request.SellerId, cancellationToken);

        // ✅ PERFORMANCE: Database'de sum yap (memory'de işlem YASAK)
        var totalAmount = await _context.Set<SellerCommission>()
            .AsNoTracking()
            .Where(sc => request.CommissionIds.Contains(sc.Id) && sc.SellerId == request.SellerId && sc.Status == CommissionStatus.Approved)
            .SumAsync(c => c.NetAmount, cancellationToken);

        if (settings != null && totalAmount < settings.MinimumPayoutAmount)
        {
            _logger.LogWarning("Payout amount below minimum. SellerId: {SellerId}, Amount: {Amount}, Minimum: {Minimum}",
                request.SellerId, totalAmount, settings.MinimumPayoutAmount);
            throw new ValidationException($"Minimum ödeme tutarı {settings.MinimumPayoutAmount}.");
        }

        // ✅ BOLUM 12.0: Magic number config'den - SellerSettings kullanımı
        var transactionFee = totalAmount * (_sellerSettings.Value.PayoutTransactionFeeRate / 100);
        var netAmount = totalAmount - transactionFee;

        var payoutNumber = await GeneratePayoutNumberAsync(cancellationToken);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        // ✅ BOLUM 12.0: Magic number config'den - SellerSettings kullanımı
        var payout = CommissionPayout.Create(
            sellerId: request.SellerId,
            payoutNumber: payoutNumber,
            totalAmount: totalAmount,
            transactionFee: transactionFee,
            paymentMethod: settings?.PaymentMethod ?? _sellerSettings.Value.DefaultPaymentMethod,
            paymentDetails: settings?.PaymentDetails);

        await _context.Set<CommissionPayout>().AddAsync(payout, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var commission in commissions)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            payout.AddItem(commission.Id);

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            commission.MarkAsPaid(payoutNumber);
        }

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        payout = await _context.Set<CommissionPayout>()
            .AsNoTracking()
            .Include(p => p.Seller)
            .Include(p => p.Items)
                .ThenInclude(i => i.Commission)
                    .ThenInclude(c => c.Order)
            .FirstOrDefaultAsync(p => p.Id == payout.Id, cancellationToken);

        _logger.LogInformation("Payout requested. PayoutId: {PayoutId}, PayoutNumber: {PayoutNumber}, Amount: {Amount}",
            payout!.Id, payoutNumber, netAmount);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<CommissionPayoutDto>(payout);
    }

    private async Task<string> GeneratePayoutNumberAsync(CancellationToken cancellationToken)
    {
        var lastPayout = await _context.Set<CommissionPayout>()
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
