using Merge.Application.DTOs.B2B;
using Merge.Application.Common;

namespace Merge.Application.Interfaces.B2B;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
public interface IB2BService
{
    // B2B Users
    Task<B2BUserDto> CreateB2BUserAsync(CreateB2BUserDto dto, CancellationToken cancellationToken = default);
    Task<B2BUserDto?> GetB2BUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<B2BUserDto?> GetB2BUserByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
    Task<PagedResult<B2BUserDto>> GetOrganizationB2BUsersAsync(Guid organizationId, string? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<bool> UpdateB2BUserAsync(Guid id, UpdateB2BUserDto dto, CancellationToken cancellationToken = default);
    Task<bool> ApproveB2BUserAsync(Guid id, Guid approvedByUserId, CancellationToken cancellationToken = default);
    Task<bool> DeleteB2BUserAsync(Guid id, CancellationToken cancellationToken = default);
    
    // Wholesale Prices
    Task<WholesalePriceDto> CreateWholesalePriceAsync(CreateWholesalePriceDto dto, CancellationToken cancellationToken = default);
    // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
    Task<PagedResult<WholesalePriceDto>> GetProductWholesalePricesAsync(Guid productId, Guid? organizationId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<decimal?> GetWholesalePriceAsync(Guid productId, int quantity, Guid? organizationId = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateWholesalePriceAsync(Guid id, CreateWholesalePriceDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteWholesalePriceAsync(Guid id, CancellationToken cancellationToken = default);
    
    // Credit Terms
    Task<CreditTermDto> CreateCreditTermAsync(CreateCreditTermDto dto, CancellationToken cancellationToken = default);
    Task<CreditTermDto?> GetCreditTermByIdAsync(Guid id, CancellationToken cancellationToken = default);
    // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
    Task<PagedResult<CreditTermDto>> GetOrganizationCreditTermsAsync(Guid organizationId, bool? isActive = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<bool> UpdateCreditTermAsync(Guid id, CreateCreditTermDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteCreditTermAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> UpdateCreditUsageAsync(Guid creditTermId, decimal amount, CancellationToken cancellationToken = default);
    
    // Volume Discounts
    Task<VolumeDiscountDto> CreateVolumeDiscountAsync(CreateVolumeDiscountDto dto, CancellationToken cancellationToken = default);
    // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
    Task<PagedResult<VolumeDiscountDto>> GetVolumeDiscountsAsync(Guid? productId = null, Guid? categoryId = null, Guid? organizationId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<decimal> CalculateVolumeDiscountAsync(Guid productId, int quantity, Guid? organizationId = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateVolumeDiscountAsync(Guid id, CreateVolumeDiscountDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteVolumeDiscountAsync(Guid id, CancellationToken cancellationToken = default);
    
    // Purchase Orders
    Task<PurchaseOrderDto> CreatePurchaseOrderAsync(Guid b2bUserId, CreatePurchaseOrderDto dto, CancellationToken cancellationToken = default);
    Task<PurchaseOrderDto?> GetPurchaseOrderByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PurchaseOrderDto?> GetPurchaseOrderByPONumberAsync(string poNumber, CancellationToken cancellationToken = default);
    // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
    Task<PagedResult<PurchaseOrderDto>> GetOrganizationPurchaseOrdersAsync(Guid organizationId, string? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
    Task<PagedResult<PurchaseOrderDto>> GetB2BUserPurchaseOrdersAsync(Guid b2bUserId, string? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<bool> SubmitPurchaseOrderAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ApprovePurchaseOrderAsync(Guid id, Guid approvedByUserId, CancellationToken cancellationToken = default);
    Task<bool> RejectPurchaseOrderAsync(Guid id, string reason, CancellationToken cancellationToken = default);
    Task<bool> CancelPurchaseOrderAsync(Guid id, CancellationToken cancellationToken = default);
    
}

