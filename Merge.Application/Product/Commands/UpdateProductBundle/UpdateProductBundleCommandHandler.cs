using MediatR;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Commands.UpdateProductBundle;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class UpdateProductBundleCommandHandler : IRequestHandler<UpdateProductBundleCommand, ProductBundleDto>
{
    private readonly Merge.Application.Interfaces.IRepository<ProductBundle> _bundleRepository;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateProductBundleCommandHandler> _logger;
    private const string CACHE_KEY_BUNDLE_BY_ID = "bundle_";
    private const string CACHE_KEY_ALL_BUNDLES = "bundles_all";
    private const string CACHE_KEY_ACTIVE_BUNDLES = "bundles_active";

    public UpdateProductBundleCommandHandler(
        Merge.Application.Interfaces.IRepository<ProductBundle> bundleRepository,
        IDbContext context,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        IMapper mapper,
        ILogger<UpdateProductBundleCommandHandler> logger)
    {
        _bundleRepository = bundleRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ProductBundleDto> Handle(UpdateProductBundleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating product bundle. BundleId: {BundleId}", request.Id);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var bundle = await _bundleRepository.GetByIdAsync(request.Id, cancellationToken);
            if (bundle == null)
            {
                throw new NotFoundException("Paket", request.Id);
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
            bundle.Update(
                request.Name,
                request.Description,
                request.BundlePrice,
                bundle.OriginalTotalPrice,
                request.ImageUrl,
                request.StartDate,
                request.EndDate);

            if (request.IsActive)
            {
                bundle.Activate();
            }
            else
            {
                bundle.Deactivate();
            }

            await _bundleRepository.UpdateAsync(bundle, cancellationToken);
            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ PERFORMANCE: Reload with all includes in one query instead of multiple LoadAsync calls (N+1 fix)
            var reloadedBundle = await _context.Set<ProductBundle>()
                .AsNoTracking()
                .Include(b => b.BundleItems)
                    .ThenInclude(bi => bi.Product)
                .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

            if (reloadedBundle == null)
            {
                _logger.LogWarning("Product bundle {BundleId} not found after update", request.Id);
                throw new NotFoundException("Paket", request.Id);
            }

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync($"{CACHE_KEY_BUNDLE_BY_ID}{request.Id}", cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_ALL_BUNDLES, cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_ACTIVE_BUNDLES, cancellationToken);

            _logger.LogInformation("Product bundle updated successfully. BundleId: {BundleId}", request.Id);

            return _mapper.Map<ProductBundleDto>(reloadedBundle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product bundle. BundleId: {BundleId}", request.Id);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
