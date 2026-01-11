using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.DTOs.Seller;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;

namespace Merge.Application.Seller.Commands.UpdateSellerCommissionSettings;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class UpdateSellerCommissionSettingsCommandHandler : IRequestHandler<UpdateSellerCommissionSettingsCommand, SellerCommissionSettingsDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IOptions<SellerSettings> _sellerSettings;
    private readonly ILogger<UpdateSellerCommissionSettingsCommandHandler> _logger;

    public UpdateSellerCommissionSettingsCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IOptions<SellerSettings> sellerSettings,
        ILogger<UpdateSellerCommissionSettingsCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _sellerSettings = sellerSettings;
        _logger = logger;
    }

    public async Task<SellerCommissionSettingsDto> Handle(UpdateSellerCommissionSettingsCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Updating seller commission settings. SellerId: {SellerId}", request.SellerId);

        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var settings = await _context.Set<SellerCommissionSettings>()
            .FirstOrDefaultAsync(s => s.SellerId == request.SellerId, cancellationToken);

        if (settings == null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            // ✅ BOLUM 12.0: Magic number config'den - SellerSettings kullanımı
            settings = SellerCommissionSettings.Create(
                sellerId: request.SellerId,
                minimumPayoutAmount: request.MinimumPayoutAmount ?? _sellerSettings.Value.DefaultMinimumPayoutAmount);
            await _context.Set<SellerCommissionSettings>().AddAsync(settings, cancellationToken);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        if (request.CustomCommissionRate.HasValue || request.UseCustomRate.HasValue)
        {
            settings.UpdateCustomCommissionRate(
                commissionRate: request.CustomCommissionRate ?? settings.CustomCommissionRate,
                useCustomRate: request.UseCustomRate ?? settings.UseCustomRate);
        }

        if (request.MinimumPayoutAmount.HasValue)
        {
            settings.UpdateMinimumPayoutAmount(request.MinimumPayoutAmount.Value);
        }

        if (request.PaymentMethod != null || request.PaymentDetails != null)
        {
            settings.UpdatePaymentMethod(
                paymentMethod: request.PaymentMethod,
                paymentDetails: request.PaymentDetails);
        }

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seller commission settings updated. SellerId: {SellerId}", request.SellerId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<SellerCommissionSettingsDto>(settings);
    }
}
