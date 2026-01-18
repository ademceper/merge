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

        CreateMap<SellerInvoice, SellerInvoiceDto>()
        .ForMember(dest => dest.SellerName, opt => opt.MapFrom(src => 
        src.Seller != null ? $"{src.Seller.FirstName} {src.Seller.LastName}" : string.Empty))
        .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
        .AfterMap((src, dest) =>
        {
        dest.Items = !string.IsNullOrEmpty(src.InvoiceData)
        ? JsonSerializer.Deserialize<List<InvoiceItemDto>>(src.InvoiceData) ?? new List<InvoiceItemDto>()
        : new List<InvoiceItemDto>();
        });

        CreateMap<SellerCommission, SellerCommissionDto>()
        .ForMember(dest => dest.SellerName, opt => opt.MapFrom(src => 
        src.Seller != null ? $"{src.Seller.FirstName} {src.Seller.LastName}" : "Unknown"))
        .ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => 
        src.Order != null ? src.Order.OrderNumber : "Unknown"))
        .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

        CreateMap<CommissionPayout, CommissionPayoutDto>()
        .ForMember(dest => dest.SellerName, opt => opt.MapFrom(src => 
        src.Seller != null ? $"{src.Seller.FirstName} {src.Seller.LastName}" : "Unknown"))
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
        dest.Settings = !string.IsNullOrEmpty(src.Settings)
        ? JsonSerializer.Deserialize<StoreSettingsDto>(src.Settings)
        : null;
        });


    }
}
