using Merge.Application.DTOs.B2B;

namespace Merge.Application.Interfaces.B2B;

public interface IB2BService
{
    // B2B Users
    Task<B2BUserDto> CreateB2BUserAsync(CreateB2BUserDto dto);
    Task<B2BUserDto?> GetB2BUserByIdAsync(Guid id);
    Task<B2BUserDto?> GetB2BUserByUserIdAsync(Guid userId);
    Task<IEnumerable<B2BUserDto>> GetOrganizationB2BUsersAsync(Guid organizationId, string? status = null);
    Task<bool> UpdateB2BUserAsync(Guid id, UpdateB2BUserDto dto);
    Task<bool> ApproveB2BUserAsync(Guid id, Guid approvedByUserId);
    Task<bool> DeleteB2BUserAsync(Guid id);
    
    // Wholesale Prices
    Task<WholesalePriceDto> CreateWholesalePriceAsync(CreateWholesalePriceDto dto);
    Task<IEnumerable<WholesalePriceDto>> GetProductWholesalePricesAsync(Guid productId, Guid? organizationId = null);
    Task<decimal?> GetWholesalePriceAsync(Guid productId, int quantity, Guid? organizationId = null);
    Task<bool> UpdateWholesalePriceAsync(Guid id, CreateWholesalePriceDto dto);
    Task<bool> DeleteWholesalePriceAsync(Guid id);
    
    // Credit Terms
    Task<CreditTermDto> CreateCreditTermAsync(CreateCreditTermDto dto);
    Task<CreditTermDto?> GetCreditTermByIdAsync(Guid id);
    Task<IEnumerable<CreditTermDto>> GetOrganizationCreditTermsAsync(Guid organizationId, bool? isActive = null);
    Task<bool> UpdateCreditTermAsync(Guid id, CreateCreditTermDto dto);
    Task<bool> DeleteCreditTermAsync(Guid id);
    Task<bool> UpdateCreditUsageAsync(Guid creditTermId, decimal amount);
    
    // Purchase Orders
    Task<PurchaseOrderDto> CreatePurchaseOrderAsync(Guid b2bUserId, CreatePurchaseOrderDto dto);
    Task<PurchaseOrderDto?> GetPurchaseOrderByIdAsync(Guid id);
    Task<PurchaseOrderDto?> GetPurchaseOrderByPONumberAsync(string poNumber);
    Task<IEnumerable<PurchaseOrderDto>> GetOrganizationPurchaseOrdersAsync(Guid organizationId, string? status = null);
    Task<IEnumerable<PurchaseOrderDto>> GetB2BUserPurchaseOrdersAsync(Guid b2bUserId, string? status = null);
    Task<bool> SubmitPurchaseOrderAsync(Guid id);
    Task<bool> ApprovePurchaseOrderAsync(Guid id, Guid approvedByUserId);
    Task<bool> RejectPurchaseOrderAsync(Guid id, string reason);
    Task<bool> CancelPurchaseOrderAsync(Guid id);
    
    // Volume Discounts
    Task<VolumeDiscountDto> CreateVolumeDiscountAsync(CreateVolumeDiscountDto dto);
    Task<IEnumerable<VolumeDiscountDto>> GetVolumeDiscountsAsync(Guid? productId = null, Guid? categoryId = null, Guid? organizationId = null);
    Task<decimal> CalculateVolumeDiscountAsync(Guid productId, int quantity, Guid? organizationId = null);
    Task<bool> UpdateVolumeDiscountAsync(Guid id, CreateVolumeDiscountDto dto);
    Task<bool> DeleteVolumeDiscountAsync(Guid id);
}

