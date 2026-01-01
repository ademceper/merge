using Merge.Application.Common;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Interfaces.Marketing;

public interface ICouponService
{
    Task<CouponDto?> GetByCodeAsync(string code);
    Task<CouponDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<CouponDto>> GetAllAsync();
    Task<PagedResult<CouponDto>> GetAllAsync(int page, int pageSize);
    Task<decimal> CalculateDiscountAsync(string couponCode, decimal orderAmount, Guid? userId = null, List<Guid>? productIds = null);
    Task<CouponDto> CreateAsync(CouponDto couponDto);
    Task<CouponDto> UpdateAsync(Guid id, CouponDto couponDto);
    Task<bool> DeleteAsync(Guid id);
}

