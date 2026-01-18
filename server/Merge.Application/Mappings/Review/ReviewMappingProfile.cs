using AutoMapper;
using Merge.Application.DTOs.Review;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketplace;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using System.Text.Json;

namespace Merge.Application.Mappings.Review;

public class ReviewMappingProfile : Profile
{
    public ReviewMappingProfile()
    {
        CreateMap<ReviewEntity, ReviewDto>()
        .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : string.Empty))
        .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty));
        CreateMap<CreateReviewDto, ReviewEntity>();
        CreateMap<UpdateReviewDto, ReviewEntity>();

        // TrustBadge mappings
        CreateMap<TrustBadge, TrustBadgeDto>()
        .AfterMap((src, dest) =>
        {
        dest.Criteria = !string.IsNullOrEmpty(src.Criteria)
        ? JsonSerializer.Deserialize<TrustBadgeSettingsDto>(src.Criteria)
        : null;
        });
        CreateMap<CreateTrustBadgeDto, TrustBadge>();
        CreateMap<UpdateTrustBadgeDto, TrustBadge>();

        // SellerTrustBadge mappings
        CreateMap<SellerTrustBadge, SellerTrustBadgeDto>()
        .ForMember(dest => dest.SellerName, opt => opt.MapFrom(src => src.Seller != null ? src.Seller.StoreName : string.Empty));

        // ProductTrustBadge mappings
        CreateMap<ProductTrustBadge, ProductTrustBadgeDto>()
        .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty));

        // ReviewHelpfulnessStatsDto - Review entity'den mapping (computed properties)
        CreateMap<ReviewEntity, ReviewHelpfulnessStatsDto>()
        .ForMember(dest => dest.ReviewId, opt => opt.MapFrom(src => src.Id))
        .ForMember(dest => dest.TotalVotes, opt => opt.MapFrom(src => src.HelpfulCount + src.UnhelpfulCount))
        .ForMember(dest => dest.HelpfulPercentage, opt => opt.MapFrom(src => 
        (src.HelpfulCount + src.UnhelpfulCount) > 0 
        ? Math.Round((decimal)src.HelpfulCount / (src.HelpfulCount + src.UnhelpfulCount) * 100, 2) 
        : 0))
        .ForMember(dest => dest.UserVote, opt => opt.Ignore()); // Set manually in service

    }
}
