using Merge.Domain.Enums;

namespace Merge.Application.DTOs.Seller;

/// <summary>
/// Partial update DTO for Store (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchStoreDto
{
    public string? StoreName { get; init; }
    public string? Description { get; init; }
    public string? LogoUrl { get; init; }
    public string? BannerUrl { get; init; }
    public string? ContactEmail { get; init; }
    public string? ContactPhone { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? Country { get; init; }
    public string? PostalCode { get; init; }
    public EntityStatus? Status { get; init; }
    public bool? IsPrimary { get; init; }
    public StoreSettingsDto? Settings { get; init; }
}
