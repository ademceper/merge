using Merge.Domain.Enums;
using Merge.Domain.Modules.Identity;
using Merge.Domain.ValueObjects;

namespace Merge.Application.DTOs.Seller;

public record StoreDto
{
    public Guid Id { get; init; }
    public Guid SellerId { get; init; }
    public string SellerName { get; init; } = string.Empty;
    public string StoreName { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? LogoUrl { get; init; }
    public string? BannerUrl { get; init; }
    public string? ContactEmail { get; init; }
    public string? ContactPhone { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? Country { get; init; }
    public string? PostalCode { get; init; }
    public EntityStatus Status { get; init; }
    public bool IsPrimary { get; init; }
    public bool IsVerified { get; init; }
    public DateTime? VerifiedAt { get; init; }
    public StoreSettingsDto? Settings { get; set; }
    public int ProductCount { get; set; }
    public DateTime CreatedAt { get; init; }
}
