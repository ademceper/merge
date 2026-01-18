using AutoMapper;
using Merge.Application.DTOs.B2B;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Payment;
using Merge.Domain.Modules.Ordering;
using System.Text.Json;

namespace Merge.Application.Mappings.B2B;

public class B2BMappingProfile : Profile
{
    public B2BMappingProfile()
    {
        // B2B mappings
        CreateMap<B2BUser, B2BUserDto>()
        .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => 
        src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : string.Empty))
        .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : string.Empty))
        .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.Organization != null ? src.Organization.Name : string.Empty))
        .ForMember(dest => dest.AvailableCredit, opt => opt.MapFrom(src => 
        src.CreditLimit.HasValue && src.UsedCredit.HasValue 
        ? src.CreditLimit.Value - src.UsedCredit.Value 
        : (decimal?)null))
        .AfterMap((src, dest) => 
        {
        // Typed DTO kullanılıyor
        dest.Settings = !string.IsNullOrEmpty(src.Settings) 
        ? JsonSerializer.Deserialize<B2BUserSettingsDto>(src.Settings!) 
        : null;
        });

        CreateMap<WholesalePrice, WholesalePriceDto>()
        .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
        .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.Organization != null ? src.Organization.Name : null));

        CreateMap<CreditTerm, CreditTermDto>()
        .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.Organization != null ? src.Organization.Name : string.Empty))
        .ForMember(dest => dest.AvailableCredit, opt => opt.MapFrom(src => 
        src.CreditLimit.HasValue && src.UsedCredit.HasValue 
        ? src.CreditLimit.Value - src.UsedCredit.Value 
        : (decimal?)null));

        CreateMap<PurchaseOrder, PurchaseOrderDto>()
        .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.Organization != null ? src.Organization.Name : string.Empty))
        .ForMember(dest => dest.B2BUserName, opt => opt.MapFrom(src => 
        src.B2BUser != null && src.B2BUser.User != null 
        ? $"{src.B2BUser.User.FirstName} {src.B2BUser.User.LastName}" 
        : null))
        .ForMember(dest => dest.CreditTermName, opt => opt.MapFrom(src => src.CreditTerm != null ? src.CreditTerm.Name : null));

        CreateMap<PurchaseOrderItem, PurchaseOrderItemDto>()
        .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
        .ForMember(dest => dest.ProductSKU, opt => opt.MapFrom(src => src.Product != null ? src.Product.SKU : string.Empty));

        CreateMap<VolumeDiscount, VolumeDiscountDto>()
        .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
        .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
        .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.Organization != null ? src.Organization.Name : null));


    }
}
