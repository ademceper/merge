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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class UpdateStoreCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<UpdateStoreCommandHandler> logger) : IRequestHandler<UpdateStoreCommand, bool>
{

    public async Task<bool> Handle(UpdateStoreCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Updating store. StoreId: {StoreId}", request.StoreId);

        if (request.Dto == null)
        {
            throw new ArgumentNullException(nameof(request.Dto));
        }

        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var store = await context.Set<Store>()
            .FirstOrDefaultAsync(s => s.Id == request.StoreId, cancellationToken);

        if (store == null)
        {
            logger.LogWarning("Store not found. StoreId: {StoreId}", request.StoreId);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
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

        // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
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
            // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
            // Unset other primary stores
            var existingPrimary = await context.Set<Store>()
                .Where(s => s.SellerId == store.SellerId && s.IsPrimary && s.Id != request.StoreId)
                .ToListAsync(cancellationToken);

            foreach (var s in existingPrimary)
            {
                // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
                s.RemovePrimaryStatus();
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            store.SetAsPrimary();
        }
        else if (request.Dto.IsPrimary.HasValue && !request.Dto.IsPrimary.Value)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            store.RemovePrimaryStatus();
        }
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Store updated. StoreId: {StoreId}", request.StoreId);

        return true;
    }
}
