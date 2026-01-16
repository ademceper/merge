using Merge.Domain.Enums;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.DTOs.Order;

/// <summary>
/// Partial update DTO for Order Status (PATCH support)
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchOrderStatusDto
{
    public OrderStatus? Status { get; init; }
}
