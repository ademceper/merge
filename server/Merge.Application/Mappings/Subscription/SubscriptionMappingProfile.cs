using AutoMapper;
using Merge.Application.DTOs.Subscription;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Modules.Payment;
using Merge.Domain.Enums;
using System.Text.Json;

namespace Merge.Application.Mappings.Subscription;

public class SubscriptionMappingProfile : Profile
{
    public SubscriptionMappingProfile()
    {
        // Subscription Domain Mappings
        CreateMap<SubscriptionPlan, SubscriptionPlanDto>()
        .ForMember(dest => dest.Features, opt => opt.Ignore()) // Will be set in AfterMap
        .ForMember(dest => dest.SubscriberCount, opt => opt.Ignore()) // Will be set in QueryHandler after batch loading
        .AfterMap((src, dest) =>
        {
        // ✅ SECURITY: Dictionary<string,object> yerine typed DTO kullaniyoruz
        dest.Features = !string.IsNullOrEmpty(src.Features)
        ? JsonSerializer.Deserialize<SubscriptionPlanFeaturesDto>(src.Features)
        : null;
        });

        CreateMap<UserSubscription, UserSubscriptionDto>()
        .ForMember(dest => dest.UserName, opt => opt.MapFrom(src =>
        src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : string.Empty))
        .ForMember(dest => dest.PlanName, opt => opt.MapFrom(src =>
        src.SubscriptionPlan != null ? src.SubscriptionPlan.Name : string.Empty))
        // ✅ BOLUM 1.2: Enum kullanımı (string YASAK) - Status enum'dan enum'a direkt map edilir
        .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
        .ForMember(dest => dest.IsTrial, opt => opt.MapFrom(src => src.Status == SubscriptionStatus.Trial))
        .ForMember(dest => dest.DaysRemaining, opt => opt.MapFrom(src =>
        src.EndDate > DateTime.UtcNow ? (int)(src.EndDate - DateTime.UtcNow).TotalDays : 0))
        .ForMember(dest => dest.RecentPayments, opt => opt.Ignore()); // Will be set in QueryHandler after batch loading

        CreateMap<SubscriptionPayment, SubscriptionPaymentDto>()
        // ✅ BOLUM 1.2: Enum kullanımı (string YASAK) - PaymentStatus enum'dan enum'a direkt map edilir
        .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.PaymentStatus));

        CreateMap<SubscriptionUsage, SubscriptionUsageDto>()
        .ForMember(dest => dest.Remaining, opt => opt.MapFrom(src =>
        src.Limit.HasValue ? src.Limit.Value - src.UsageCount : (int?)null));


    }
}
