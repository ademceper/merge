using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.DTOs.Seller;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Commands.UpdateSellerCommissionSettings;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class UpdateSellerCommissionSettingsCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, IOptions<SellerSettings> sellerSettings, ILogger<UpdateSellerCommissionSettingsCommandHandler> logger) : IRequestHandler<UpdateSellerCommissionSettingsCommand, SellerCommissionSettingsDto>
{
    private readonly SellerSettings sellerConfig = sellerSettings.Value;

    public async Task<SellerCommissionSettingsDto> Handle(UpdateSellerCommissionSettingsCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Updating seller commission settings. SellerId: {SellerId}", request.SellerId);

        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var settings = await context.Set<SellerCommissionSettings>()
            .FirstOrDefaultAsync(s => s.SellerId == request.SellerId, cancellationToken);

        if (settings == null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            // ✅ BOLUM 12.0: Magic number config'den - SellerSettings kullanımı
            settings = SellerCommissionSettings.Create(
                sellerId: request.SellerId,
                minimumPayoutAmount: request.MinimumPayoutAmount ?? sellerConfig.DefaultMinimumPayoutAmount);
            await context.Set<SellerCommissionSettings>().AddAsync(settings, cancellationToken);
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
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seller commission settings updated. SellerId: {SellerId}", request.SellerId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<SellerCommissionSettingsDto>(settings);
    }
}
