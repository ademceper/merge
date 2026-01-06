namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record ReportDto(
    Guid Id,
    string Name,
    string Description,
    string Type,
    string GeneratedBy, // User name for display
    Guid GeneratedByUserId, // User ID for authorization
    DateTime StartDate,
    DateTime EndDate,
    string Format,
    string Status,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    string? FilePath
);
