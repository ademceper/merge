using AutoMapper;

namespace Merge.Application.Mappings;

/// <summary>
/// Main mapping profile that aggregates all domain-specific mapping profiles.
/// This class follows Single Responsibility Principle by delegating to domain-specific profiles.
/// 
/// ✅ ARCHITECTURE: Domain-specific mapping profiles
/// Each domain has its own mapping profile for better maintainability and Single Responsibility Principle.
/// AutoMapper automatically discovers all Profile classes in the assembly via AddAutoMapper(assembly).
/// 
/// Domain Profiles:
/// - CatalogMappingProfile (Catalog domain)
/// - IdentityMappingProfile (Identity domain)
/// - OrderingMappingProfile (Ordering domain)
/// - MarketingMappingProfile (Marketing domain)
/// - PaymentMappingProfile (Payment domain)
/// - SupportMappingProfile (Support domain)
/// - ContentMappingProfile (Content domain)
/// - AnalyticsMappingProfile (Analytics domain)
/// - B2BMappingProfile (B2B domain)
/// - NotificationsMappingProfile (Notifications domain)
/// - InventoryMappingProfile (Inventory domain)
/// - ReviewMappingProfile (Review domain)
/// - GovernanceMappingProfile (Governance domain)
/// - InternationalMappingProfile (International domain)
/// - LiveCommerceMappingProfile (LiveCommerce domain)
/// - OrganizationMappingProfile (Organization domain)
/// - SecurityMappingProfile (Security domain)
/// - SellerMappingProfile (Seller domain)
/// - SubscriptionMappingProfile (Subscription domain)
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ✅ ARCHITECTURE: Domain-specific mapping profiles
        // Each domain has its own mapping profile for better maintainability and Single Responsibility Principle
        
        // Note: AutoMapper automatically discovers all Profile classes in the assembly
        // Domain-specific profiles are registered automatically via AddAutoMapper(assembly)
        // This profile remains for backward compatibility and can be removed if all mappings are moved to domain profiles
    }
}
