using AutoMapper;
using ProductEntity = Merge.Domain.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Marketing;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using OrderEntity = Merge.Domain.Entities.Order;
using Merge.Domain.ValueObjects;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;


namespace Merge.Application.Services.Marketing;

public class CouponService : ICouponService
{
    private readonly IRepository<Coupon> _couponRepository;
    private readonly IRepository<CouponUsage> _couponUsageRepository;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CouponService> _logger;

    public CouponService(
        IRepository<Coupon> couponRepository,
        IRepository<CouponUsage> couponUsageRepository,
        IDbContext context,
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

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<CouponDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var coupon = await _context.Set<Coupon>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == code.ToUpper(), cancellationToken);

        return coupon == null ? null : _mapper.Map<CouponDto>(coupon);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<CouponDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var coupon = await _couponRepository.GetByIdAsync(id);
        return coupon == null ? null : _mapper.Map<CouponDto>(coupon);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<CouponDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var coupons = await _context.Set<Coupon>()
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<CouponDto>>(coupons);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<PagedResult<CouponDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<Coupon>()
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var coupons = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<CouponDto>
        {
            Items = _mapper.Map<List<CouponDto>>(coupons),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<decimal> CalculateDiscountAsync(string couponCode, decimal orderAmount, Guid? userId = null, List<Guid>? productIds = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(couponCode))
        {
            throw new ValidationException("Kupon kodu boş olamaz.");
        }

        if (orderAmount <= 0)
        {
            throw new ValidationException("Sipariş tutarı 0'dan büyük olmalıdır.");
        }

        var coupon = await GetByCodeAsync(couponCode, cancellationToken);
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
            var hasOrder = await _context.Set<OrderEntity>().AsNoTracking().AnyAsync(o => o.UserId == userId.Value, cancellationToken);
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

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<CouponDto> CreateAsync(CouponDto couponDto, CancellationToken cancellationToken = default)
    {
        if (couponDto == null)
        {
            throw new ArgumentNullException(nameof(couponDto));
        }

        if (string.IsNullOrWhiteSpace(couponDto.Code))
        {
            throw new ValidationException("Kupon kodu boş olamaz.");
        }

        var existing = await _context.Set<Coupon>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == couponDto.Code.ToUpper(), cancellationToken);

        if (existing != null)
        {
            throw new BusinessException("Bu kupon kodu zaten kullanılıyor.");
        }

        _logger.LogInformation("Creating new coupon with code: {CouponCode}", couponDto.Code);

        var coupon = _mapper.Map<Coupon>(couponDto);
        coupon = await _couponRepository.AddAsync(coupon);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully created coupon {CouponId} with code: {CouponCode}", coupon.Id, coupon.Code);

        return _mapper.Map<CouponDto>(coupon);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<CouponDto> UpdateAsync(Guid id, CouponDto couponDto, CancellationToken cancellationToken = default)
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

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
        coupon.UpdateCode(couponDto.Code);
        coupon.UpdateDescription(couponDto.Description);
        
        // ✅ BOLUM 1.1: Rich Domain Model - DiscountAmount decimal (nullable değil)
        if (couponDto.DiscountAmount > 0)
        {
            var discountAmount = new Money(couponDto.DiscountAmount);
            coupon.SetDiscountAmount(discountAmount);
        }
        else
        {
            coupon.SetDiscountAmount(null);
        }
        
        if (couponDto.DiscountPercentage.HasValue)
        {
            var discountPercentage = new Percentage(couponDto.DiscountPercentage.Value);
            coupon.SetDiscountPercentage(discountPercentage);
        }
        else
        {
            coupon.SetDiscountPercentage(null);
        }
        
        if (couponDto.MinimumPurchaseAmount.HasValue)
        {
            var minimumPurchaseAmount = new Money(couponDto.MinimumPurchaseAmount.Value);
            coupon.SetMinimumPurchaseAmount(minimumPurchaseAmount);
        }
        else
        {
            coupon.SetMinimumPurchaseAmount(null);
        }
        
        if (couponDto.MaximumDiscountAmount.HasValue)
        {
            var maximumDiscountAmount = new Money(couponDto.MaximumDiscountAmount.Value);
            coupon.SetMaximumDiscountAmount(maximumDiscountAmount);
        }
        else
        {
            coupon.SetMaximumDiscountAmount(null);
        }
        
        coupon.UpdateDates(couponDto.StartDate, couponDto.EndDate);
        coupon.SetUsageLimit(couponDto.UsageLimit);
        coupon.SetApplicableCategoryIds(couponDto.ApplicableCategoryIds);
        coupon.SetApplicableProductIds(couponDto.ApplicableProductIds);
        coupon.SetForNewUsersOnly(couponDto.IsForNewUsersOnly);
        
        if (couponDto.IsActive)
            coupon.Activate();
        else
            coupon.Deactivate();

        await _couponRepository.UpdateAsync(coupon);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully updated coupon {CouponId}", id);

        return _mapper.Map<CouponDto>(coupon);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting coupon {CouponId}", id);

        var coupon = await _couponRepository.GetByIdAsync(id);
        if (coupon == null)
        {
            return false;
        }

        await _couponRepository.DeleteAsync(coupon);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully deleted coupon {CouponId}", id);

        return true;
    }
}

