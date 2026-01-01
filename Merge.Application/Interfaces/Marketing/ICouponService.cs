using Merge.Application.Common;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Interfaces.Marketing;

public interface ICouponService
{
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    Task<CouponDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<CouponDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<CouponDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<CouponDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<decimal> CalculateDiscountAsync(string couponCode, decimal orderAmount, Guid? userId = null, List<Guid>? productIds = null, CancellationToken cancellationToken = default);
    Task<CouponDto> CreateAsync(CouponDto couponDto, CancellationToken cancellationToken = default);
    Task<CouponDto> UpdateAsync(Guid id, CouponDto couponDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

