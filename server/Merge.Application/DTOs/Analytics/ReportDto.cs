using Merge.Domain.Modules.Identity;
namespace Merge.Application.DTOs.Analytics;

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
