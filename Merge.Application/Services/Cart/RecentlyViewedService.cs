using AutoMapper;
using CartEntity = Merge.Domain.Entities.Cart;
using ProductEntity = Merge.Domain.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Cart;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using Merge.Application.DTOs.Product;


namespace Merge.Application.Services.Cart;

public class RecentlyViewedService : IRecentlyViewedService
{
    private readonly IRepository<RecentlyViewedProduct> _recentlyViewedRepository;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public RecentlyViewedService(
        IRepository<RecentlyViewedProduct> recentlyViewedRepository,
        ApplicationDbContext context,
        IMapper mapper,
        IUnitOfWork unitOfWork)
    {
        _recentlyViewedRepository = recentlyViewedRepository;
        _context = context;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ProductDto>> GetRecentlyViewedAsync(Guid userId, int count = 20)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !rvp.IsDeleted and !rvp.Product.IsDeleted checks (Global Query Filter handles it)
        var recentlyViewed = await _context.RecentlyViewedProducts
            .AsNoTracking()
            .Include(rvp => rvp.Product)
                .ThenInclude(p => p.Category)
            .Where(rvp => rvp.UserId == userId && rvp.Product.IsActive)
            .OrderByDescending(rvp => rvp.ViewedAt)
            .Take(count)
            .Select(rvp => rvp.Product)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return _mapper.Map<IEnumerable<ProductDto>>(recentlyViewed);
    }

    public async Task AddToRecentlyViewedAsync(Guid userId, Guid productId)
    {
        // ✅ PERFORMANCE: Removed manual !rvp.IsDeleted check (Global Query Filter handles it)
        var existing = await _context.RecentlyViewedProducts
            .FirstOrDefaultAsync(rvp => rvp.UserId == userId && 
                                  rvp.ProductId == productId);

        if (existing != null)
        {
            // Zaten varsa sadece tarihi güncelle
            existing.ViewedAt = DateTime.UtcNow;
            await _recentlyViewedRepository.UpdateAsync(existing);
            await _unitOfWork.SaveChangesAsync();
        }
        else
        {
            // Yeni kayıt oluştur
            var recentlyViewed = new RecentlyViewedProduct
            {
                UserId = userId,
                ProductId = productId,
                ViewedAt = DateTime.UtcNow
            };

            await _recentlyViewedRepository.AddAsync(recentlyViewed);
            await _unitOfWork.SaveChangesAsync();

            // ✅ PERFORMANCE: Database'de Count yap (memory'de işlem YASAK)
            // ✅ PERFORMANCE: Removed manual !rvp.IsDeleted check (Global Query Filter handles it)
            var count = await _context.RecentlyViewedProducts
                .CountAsync(rvp => rvp.UserId == userId);

            if (count > 100)
            {
                // ✅ PERFORMANCE: Bulk delete instead of foreach DeleteAsync (N+1 fix)
                // ✅ PERFORMANCE: Removed manual !rvp.IsDeleted check (Global Query Filter handles it)
                var oldest = await _context.RecentlyViewedProducts
                    .Where(rvp => rvp.UserId == userId)
                    .OrderBy(rvp => rvp.ViewedAt)
                    .Take(count - 100)
                    .ToListAsync();

                foreach (var item in oldest)
                {
                    item.IsDeleted = true;
                    item.UpdatedAt = DateTime.UtcNow;
                }

                await _unitOfWork.SaveChangesAsync(); // ✅ CRITICAL FIX: Single SaveChanges
            }
        }
    }

    public async Task ClearRecentlyViewedAsync(Guid userId)
    {
        // ✅ PERFORMANCE: Bulk delete instead of foreach DeleteAsync (N+1 fix)
        // ✅ PERFORMANCE: Removed manual !rvp.IsDeleted check (Global Query Filter handles it)
        var recentlyViewed = await _context.RecentlyViewedProducts
            .Where(rvp => rvp.UserId == userId)
            .ToListAsync();

        foreach (var item in recentlyViewed)
        {
            item.IsDeleted = true;
            item.UpdatedAt = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync(); // ✅ CRITICAL FIX: Single SaveChanges
    }
}

