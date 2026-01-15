using AutoMapper;
using Merge.Application.DTOs.LiveCommerce;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Mappings.LiveCommerce;

public class LiveCommerceMappingProfile : Profile
{
    public LiveCommerceMappingProfile()
    {
        // LiveCommerce mappings
        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
        CreateMap<LiveStream, LiveStreamDto>()
        .ConstructUsing(src => new LiveStreamDto(
        src.Id,
        src.SellerId,
        src.Seller != null ? src.Seller.StoreName : string.Empty,
        src.Title,
        src.Description,
        src.Status.ToString(),
        src.ScheduledStartTime,
        src.ActualStartTime,
        src.EndTime,
        src.StreamUrl,
        src.StreamKey,
        src.ThumbnailUrl,
        src.ViewerCount,
        src.PeakViewerCount,
        src.TotalViewerCount,
        src.OrderCount,
        src.Revenue,
        src.IsActive,
        src.Category,
        src.Tags,
        src.Products != null ? src.Products.Select(p => new LiveStreamProductDto(
        p.Id,
        p.ProductId,
        p.Product != null ? p.Product.Name : string.Empty,
        p.Product != null ? p.Product.ImageUrl : null,
        p.Product != null ? p.Product.Price : null,
        p.SpecialPrice,
        p.DisplayOrder,
        p.IsHighlighted,
        p.ShowcasedAt,
        p.ViewCount,
        p.ClickCount,
        p.OrderCount,
        p.ShowcaseNotes
        )).ToList().AsReadOnly() : new List<LiveStreamProductDto>().AsReadOnly(),
        src.CreatedAt,
        src.UpdatedAt));

        CreateMap<LiveStreamProduct, LiveStreamProductDto>()
        .ConstructUsing(src => new LiveStreamProductDto(
        src.Id,
        src.ProductId,
        src.Product != null ? src.Product.Name : string.Empty,
        src.Product != null ? src.Product.ImageUrl : null,
        src.Product != null ? src.Product.Price : null,
        src.SpecialPrice,
        src.DisplayOrder,
        src.IsHighlighted,
        src.ShowcasedAt,
        src.ViewCount,
        src.ClickCount,
        src.OrderCount,
        src.ShowcaseNotes));

        CreateMap<LiveStreamOrder, LiveStreamOrderDto>()
        .ConstructUsing(src => new LiveStreamOrderDto(
        src.Id,
        src.LiveStreamId,
        src.OrderId,
        src.ProductId,
        src.OrderAmount,
        src.CreatedAt));


    }
}
