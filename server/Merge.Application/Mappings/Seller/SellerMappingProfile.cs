using AutoMapper;
using Merge.Application.DTOs.Seller;
using Merge.Domain.Modules.Marketplace;
using System.Text.Json;

namespace Merge.Application.Mappings.Seller;

public class SellerMappingProfile : Profile
{
    public SellerMappingProfile()
    {
        // Seller domain mappings
        CreateMap<SellerTransaction, SellerTransactionDto>();

        // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
        CreateMap<SellerInvoice, SellerInvoiceDto>()
        .ForMember(dest => dest.SellerName, opt => opt.MapFrom(src => 
        src.Seller != null ? $"{src.Seller.FirstName} {src.Seller.LastName}" : string.Empty))
        // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - Status enum'dan enum'a direkt map edilir
        .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
        .AfterMap((src, dest) =>
        {
        dest.Items = !string.IsNullOrEmpty(src.InvoiceData)
        ? JsonSerializer.Deserialize<List<InvoiceItemDto>>(src.InvoiceData) ?? new List<InvoiceItemDto>()
        : new List<InvoiceItemDto>();
        });

        // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
        CreateMap<SellerCommission, SellerCommissionDto>()
        .ForMember(dest => dest.SellerName, opt => opt.MapFrom(src => 
        src.Seller != null ? $"{src.Seller.FirstName} {src.Seller.LastName}" : "Unknown"))
        .ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => 
        src.Order != null ? src.Order.OrderNumber : "Unknown"))
        // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - Status enum'dan enum'a direkt map edilir
        .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

        // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
        CreateMap<CommissionPayout, CommissionPayoutDto>()
        .ForMember(dest => dest.SellerName, opt => opt.MapFrom(src => 
        src.Seller != null ? $"{src.Seller.FirstName} {src.Seller.LastName}" : "Unknown"))
        // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - Status enum'dan enum'a direkt map edilir
        .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
        .ForMember(dest => dest.Commissions, opt => opt.MapFrom(src => 
        src.Items != null && src.Items.Any() 
        ? src.Items.Where(i => i.Commission != null).Select(i => i.Commission!)
        : new List<SellerCommission>()));

        CreateMap<CommissionTier, CommissionTierDto>();

        CreateMap<SellerCommissionSettings, SellerCommissionSettingsDto>();

        CreateMap<Store, StoreDto>()
        .ForMember(dest => dest.SellerName, opt => opt.MapFrom(src =>
        src.Seller != null ? $"{src.Seller.FirstName} {src.Seller.LastName}" : string.Empty))
        .ForMember(dest => dest.ProductCount, opt => opt.Ignore()) // Will be set in StoreService after batch loading
        .AfterMap((src, dest) =>
        {
        // ✅ SECURITY: Dictionary<string,object> yerine typed DTO kullaniyoruz
        dest.Settings = !string.IsNullOrEmpty(src.Settings)
        ? JsonSerializer.Deserialize<StoreSettingsDto>(src.Settings)
        : null;
        });


    }
}
