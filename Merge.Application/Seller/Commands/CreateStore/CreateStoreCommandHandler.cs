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
using ProductEntity = Merge.Domain.Entities.Product;

namespace Merge.Application.Seller.Commands.CreateStore;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CreateStoreCommandHandler : IRequestHandler<CreateStoreCommand, StoreDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateStoreCommandHandler> _logger;

    public CreateStoreCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateStoreCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<StoreDto> Handle(CreateStoreCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Creating store for seller {SellerId}, StoreName: {StoreName}",
            request.SellerId, request.Dto.StoreName);

        if (request.Dto == null)
        {
            throw new ArgumentNullException(nameof(request.Dto));
        }

        if (string.IsNullOrWhiteSpace(request.Dto.StoreName))
        {
            throw new ValidationException("Mağaza adı boş olamaz.");
        }

        // ✅ PERFORMANCE: Removed manual !u.IsDeleted (Global Query Filter)
        var seller = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.SellerId, cancellationToken);

        if (seller == null)
        {
            _logger.LogWarning("Seller not found. SellerId: {SellerId}", request.SellerId);
            throw new NotFoundException("Satıcı", request.SellerId);
        }

        // If this is primary, unset other primary stores
        if (request.Dto.IsPrimary)
        {
            // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
            var existingPrimary = await _context.Set<Store>()
                .Where(s => s.SellerId == request.SellerId && s.IsPrimary)
                .ToListAsync(cancellationToken);

            foreach (var primaryStore in existingPrimary)
            {
                primaryStore.RemovePrimaryStatus();
            }
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
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
            settings: request.Dto.Settings != null ? System.Text.Json.JsonSerializer.Serialize(request.Dto.Settings) : null);

        // Set as primary if requested
        if (request.Dto.IsPrimary)
        {
            store.SetAsPrimary();
        }

        await _context.Set<Store>().AddAsync(store, cancellationToken);
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        var reloadedStore = await _context.Set<Store>()
            .AsNoTracking()
            .Include(s => s.Seller)
            .FirstOrDefaultAsync(s => s.Id == store.Id, cancellationToken);

        if (reloadedStore == null)
        {
            _logger.LogWarning("Store {StoreId} not found after creation", store.Id);
            return _mapper.Map<StoreDto>(store);
        }

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var storeDto = _mapper.Map<StoreDto>(reloadedStore);
        
        // ✅ PERFORMANCE: ProductCount için database'de count (N+1 fix)
        // ✅ FIX: Record immutable - with expression kullan
        var productCount = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(p => p.StoreId.HasValue && p.StoreId.Value == reloadedStore.Id, cancellationToken);
        
        _logger.LogInformation("Store created. StoreId: {StoreId}, StoreName: {StoreName}",
            reloadedStore.Id, reloadedStore.StoreName);
        
        return storeDto with { ProductCount = productCount };
    }

}
