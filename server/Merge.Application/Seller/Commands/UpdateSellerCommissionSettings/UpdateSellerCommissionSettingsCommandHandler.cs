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

public class UpdateSellerCommissionSettingsCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, IOptions<SellerSettings> sellerSettings, ILogger<UpdateSellerCommissionSettingsCommandHandler> logger) : IRequestHandler<UpdateSellerCommissionSettingsCommand, SellerCommissionSettingsDto>
{
    private readonly SellerSettings sellerConfig = sellerSettings.Value;

    public async Task<SellerCommissionSettingsDto> Handle(UpdateSellerCommissionSettingsCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating seller commission settings. SellerId: {SellerId}", request.SellerId);

        var settings = await context.Set<SellerCommissionSettings>()
            .FirstOrDefaultAsync(s => s.SellerId == request.SellerId, cancellationToken);

        if (settings is null)
        {
            settings = SellerCommissionSettings.Create(
                sellerId: request.SellerId,
                minimumPayoutAmount: request.MinimumPayoutAmount ?? sellerConfig.DefaultMinimumPayoutAmount);
            await context.Set<SellerCommissionSettings>().AddAsync(settings, cancellationToken);
        }

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

        if (request.PaymentMethod is not null || request.PaymentDetails is not null)
        {
            settings.UpdatePaymentMethod(
                paymentMethod: request.PaymentMethod,
                paymentDetails: request.PaymentDetails);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seller commission settings updated. SellerId: {SellerId}", request.SellerId);

        return mapper.Map<SellerCommissionSettingsDto>(settings);
    }
}
