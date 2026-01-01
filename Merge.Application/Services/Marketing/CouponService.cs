using AutoMapper;
using ProductEntity = Merge.Domain.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Marketing;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;


namespace Merge.Application.Services.Marketing;

public class CouponService : ICouponService
{
    private readonly IRepository<Coupon> _couponRepository;
    private readonly IRepository<CouponUsage> _couponUsageRepository;
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CouponService> _logger;

    public CouponService(
        IRepository<Coupon> couponRepository,
        IRepository<CouponUsage> couponUsageRepository,
        ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CouponService> logger)
    {
        _couponRepository = couponRepository;
        _couponUsageRepository = couponUsageRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CouponDto?> GetByCodeAsync(string code)
    {
        var coupon = await _context.Coupons
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == code.ToUpper());

        return coupon == null ? null : _mapper.Map<CouponDto>(coupon);
    }

    public async Task<CouponDto?> GetByIdAsync(Guid id)
    {
        var coupon = await _couponRepository.GetByIdAsync(id);
        return coupon == null ? null : _mapper.Map<CouponDto>(coupon);
    }

    public async Task<IEnumerable<CouponDto>> GetAllAsync()
    {
        var coupons = await _context.Coupons
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return _mapper.Map<IEnumerable<CouponDto>>(coupons);
    }

    public async Task<PagedResult<CouponDto>> GetAllAsync(int page, int pageSize)
    {
        var query = _context.Coupons
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt);

        var totalCount = await query.CountAsync();
        var coupons = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<CouponDto>
        {
            Items = _mapper.Map<List<CouponDto>>(coupons),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<decimal> CalculateDiscountAsync(string couponCode, decimal orderAmount, Guid? userId = null, List<Guid>? productIds = null)
    {
        if (string.IsNullOrWhiteSpace(couponCode))
        {
            throw new ValidationException("Kupon kodu boş olamaz.");
        }

        if (orderAmount <= 0)
        {
            throw new ValidationException("Sipariş tutarı 0'dan büyük olmalıdır.");
        }

        var coupon = await GetByCodeAsync(couponCode);
        if (coupon == null)
        {
            throw new NotFoundException("Kupon", Guid.Empty);
        }

        if (!coupon.IsActive)
        {
            throw new BusinessException("Kupon aktif değil.");
        }

        // Tarih kontrolü
        if (DateTime.UtcNow < coupon.StartDate || DateTime.UtcNow > coupon.EndDate)
        {
            throw new BusinessException("Kupon geçerli değil.");
        }

        // Minimum alışveriş tutarı kontrolü
        if (coupon.MinimumPurchaseAmount.HasValue && orderAmount < coupon.MinimumPurchaseAmount.Value)
        {
            throw new ValidationException($"Minimum alışveriş tutarı {coupon.MinimumPurchaseAmount.Value:C} olmalıdır.");
        }

        // Kullanım limiti kontrolü
        if (coupon.UsageLimit > 0 && coupon.UsedCount >= coupon.UsageLimit)
        {
            throw new BusinessException("Kupon kullanım limitine ulaşılmış.");
        }

        // Yeni kullanıcı kontrolü
        if (coupon.IsForNewUsersOnly && userId.HasValue)
        {
            var hasOrder = await _context.Orders.AsNoTracking().AnyAsync(o => o.UserId == userId.Value);
            if (hasOrder)
            {
                throw new BusinessException("Bu kupon sadece yeni kullanıcılar için geçerlidir.");
            }
        }

        // Kategori/Ürün kontrolü
        if (productIds != null && productIds.Any())
        {
            if (coupon.ApplicableProductIds != null && coupon.ApplicableProductIds.Any())
            {
                var hasApplicableProduct = productIds.Any(id => coupon.ApplicableProductIds.Contains(id));
                if (!hasApplicableProduct)
                {
                    throw new BusinessException("Bu kupon seçilen ürünler için geçerli değil.");
                }
            }
        }

        // İndirim hesaplama
        decimal discount = 0;
        if (coupon.DiscountPercentage.HasValue)
        {
            discount = orderAmount * (coupon.DiscountPercentage.Value / 100);
            if (coupon.MaximumDiscountAmount.HasValue && discount > coupon.MaximumDiscountAmount.Value)
            {
                discount = coupon.MaximumDiscountAmount.Value;
            }
        }
        else
        {
            discount = coupon.DiscountAmount;
            if (discount > orderAmount)
            {
                discount = orderAmount;
            }
        }

        return discount;
    }

    public async Task<CouponDto> CreateAsync(CouponDto couponDto)
    {
        if (couponDto == null)
        {
            throw new ArgumentNullException(nameof(couponDto));
        }

        if (string.IsNullOrWhiteSpace(couponDto.Code))
        {
            throw new ValidationException("Kupon kodu boş olamaz.");
        }

        var existing = await _context.Coupons
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == couponDto.Code.ToUpper());

        if (existing != null)
        {
            throw new BusinessException("Bu kupon kodu zaten kullanılıyor.");
        }

        _logger.LogInformation("Creating new coupon with code: {CouponCode}", couponDto.Code);

        var coupon = _mapper.Map<Coupon>(couponDto);
        coupon = await _couponRepository.AddAsync(coupon);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Successfully created coupon {CouponId} with code: {CouponCode}", coupon.Id, coupon.Code);

        return _mapper.Map<CouponDto>(coupon);
    }

    public async Task<CouponDto> UpdateAsync(Guid id, CouponDto couponDto)
    {
        if (couponDto == null)
        {
            throw new ArgumentNullException(nameof(couponDto));
        }

        _logger.LogInformation("Updating coupon {CouponId}", id);

        var coupon = await _couponRepository.GetByIdAsync(id);
        if (coupon == null)
        {
            throw new NotFoundException("Kupon", id);
        }

        coupon.Code = couponDto.Code;
        coupon.Description = couponDto.Description;
        coupon.DiscountAmount = couponDto.DiscountAmount;
        coupon.DiscountPercentage = couponDto.DiscountPercentage;
        coupon.MinimumPurchaseAmount = couponDto.MinimumPurchaseAmount;
        coupon.MaximumDiscountAmount = couponDto.MaximumDiscountAmount;
        coupon.StartDate = couponDto.StartDate;
        coupon.EndDate = couponDto.EndDate;
        coupon.UsageLimit = couponDto.UsageLimit;
        coupon.IsActive = couponDto.IsActive;
        coupon.IsForNewUsersOnly = couponDto.IsForNewUsersOnly;
        coupon.ApplicableCategoryIds = couponDto.ApplicableCategoryIds;
        coupon.ApplicableProductIds = couponDto.ApplicableProductIds;

        await _couponRepository.UpdateAsync(coupon);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Successfully updated coupon {CouponId}", id);

        return _mapper.Map<CouponDto>(coupon);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deleting coupon {CouponId}", id);

        var coupon = await _couponRepository.GetByIdAsync(id);
        if (coupon == null)
        {
            return false;
        }

        await _couponRepository.DeleteAsync(coupon);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Successfully deleted coupon {CouponId}", id);

        return true;
    }
}

