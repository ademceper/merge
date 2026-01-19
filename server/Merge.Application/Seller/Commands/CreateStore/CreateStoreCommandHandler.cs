using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Seller;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using System.Text.RegularExpressions;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Enums;
using UserEntity = Merge.Domain.Modules.Identity.User;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Commands.CreateStore;

public class CreateStoreCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    UserManager<UserEntity> userManager,
    ILogger<CreateStoreCommandHandler> logger) : IRequestHandler<CreateStoreCommand, StoreDto>
{

    public async Task<StoreDto> Handle(CreateStoreCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating store for seller {SellerId}, StoreName: {StoreName}",
            request.SellerId, request.Dto.StoreName);

        if (request.Dto is null)
        {
            throw new ArgumentNullException(nameof(request.Dto));
        }

        if (string.IsNullOrWhiteSpace(request.Dto.StoreName))
        {
            throw new ValidationException("Mağaza adı boş olamaz.");
        }

        var seller = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.SellerId, cancellationToken);

        if (seller is null)
        {
            logger.LogWarning("Seller not found. SellerId: {SellerId}", request.SellerId);
            throw new NotFoundException("Satıcı", request.SellerId);
        }

        // If this is primary, unset other primary stores
        if (request.Dto.IsPrimary)
        {
            var existingPrimary = await context.Set<Store>()
                .Where(s => s.SellerId == request.SellerId && s.IsPrimary)
                .ToListAsync(cancellationToken);

            foreach (var primaryStore in existingPrimary)
            {
                primaryStore.RemovePrimaryStatus();
            }
        }

        var store = Store.Create(
            sellerId: request.SellerId,
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
            settings: request.Dto.Settings is not null ? System.Text.Json.JsonSerializer.Serialize(request.Dto.Settings) : null);

        // Set as primary if requested
        if (request.Dto.IsPrimary)
        {
            store.SetAsPrimary();
        }

        await context.Set<Store>().AddAsync(store, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Assign Store Owner role to seller
        var sellerEntity = await context.Users.FirstOrDefaultAsync(u => u.Id == request.SellerId, cancellationToken);
        if (sellerEntity is not null)
        {
            // Get Store Owner role
            var storeOwnerRole = await context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Name == "Store Owner" && r.RoleType == RoleType.Store, cancellationToken);

            if (storeOwnerRole is not null)
            {
                // Check if already assigned
                var existingStoreRole = await context.Set<StoreRole>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(sr => sr.StoreId == store.Id && 
                                              sr.UserId == request.SellerId && 
                                              sr.RoleId == storeOwnerRole.Id && 
                                              !sr.IsDeleted, cancellationToken);

                if (existingStoreRole is null)
                {
                    var storeRole = StoreRole.Create(
                        store.Id,
                        request.SellerId,
                        storeOwnerRole.Id,
                        request.SellerId); // Self-assigned

                    await context.Set<StoreRole>().AddAsync(storeRole, cancellationToken);
                    await unitOfWork.SaveChangesAsync(cancellationToken);
                    
                    logger.LogInformation("Store Owner role assigned to seller {SellerId} for store {StoreId}", 
                        request.SellerId, store.Id);
                }
            }
        }

        var reloadedStore = await context.Set<Store>()
            .AsNoTracking()
            .Include(s => s.Seller)
            .FirstOrDefaultAsync(s => s.Id == store.Id, cancellationToken);

        if (reloadedStore is null)
        {
            logger.LogWarning("Store {StoreId} not found after creation", store.Id);
            return mapper.Map<StoreDto>(store);
        }

        var storeDto = mapper.Map<StoreDto>(reloadedStore);
        
        var productCount = await context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(p => p.StoreId.HasValue && p.StoreId.Value == reloadedStore.Id, cancellationToken);
        
        logger.LogInformation("Store created. StoreId: {StoreId}, StoreName: {StoreName}",
            reloadedStore.Id, reloadedStore.StoreName);
        
        return storeDto with { ProductCount = productCount };
    }

}
