using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using System.Text.Json;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Commands.UpdateStore;

public class UpdateStoreCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<UpdateStoreCommandHandler> logger) : IRequestHandler<UpdateStoreCommand, bool>
{

    public async Task<bool> Handle(UpdateStoreCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating store. StoreId: {StoreId}", request.StoreId);

        if (request.Dto == null)
        {
            throw new ArgumentNullException(nameof(request.Dto));
        }

        var store = await context.Set<Store>()
            .FirstOrDefaultAsync(s => s.Id == request.StoreId, cancellationToken);

        if (store == null)
        {
            logger.LogWarning("Store not found. StoreId: {StoreId}", request.StoreId);
            return false;
        }

        store.UpdateDetails(
            storeName: request.Dto.StoreName,
            description: request.Dto.Description,
            logoUrl: request.Dto.LogoUrl,
            bannerUrl: request.Dto.BannerUrl,
            contactEmail: request.Dto.ContactEmail,
            contactPhone: request.Dto.ContactPhone,
            address: request.Dto.Address,
            city: request.Dto.City,
            country: request.Dto.Country,
            postalCode: request.Dto.PostalCode,
            settings: request.Dto.Settings != null ? JsonSerializer.Serialize(request.Dto.Settings) : null);

        if (request.Dto.Status.HasValue)
        {
            var status = request.Dto.Status.Value;
            if (status == EntityStatus.Active)
                store.Activate();
            else if (status == EntityStatus.Suspended)
                store.Suspend();
        }

        if (request.Dto.IsPrimary.HasValue && request.Dto.IsPrimary.Value)
        {
            // Unset other primary stores
            var existingPrimary = await context.Set<Store>()
                .Where(s => s.SellerId == store.SellerId && s.IsPrimary && s.Id != request.StoreId)
                .ToListAsync(cancellationToken);

            foreach (var s in existingPrimary)
            {
                s.RemovePrimaryStatus();
            }

            store.SetAsPrimary();
        }
        else if (request.Dto.IsPrimary.HasValue && !request.Dto.IsPrimary.Value)
        {
            store.RemovePrimaryStatus();
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Store updated. StoreId: {StoreId}", request.StoreId);

        return true;
    }
}
