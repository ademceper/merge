using MediatR;
using Merge.Application.DTOs.Order;

namespace Merge.Application.Order.Commands.ExportOrders;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ExportOrdersCommand(
    DateTime? StartDate,
    DateTime? EndDate,
    string? Status,
    string? PaymentStatus,
    Guid? UserId,
    bool IncludeOrderItems,
    bool IncludeAddress,
    ExportFormat Format
) : IRequest<byte[]>;

public enum ExportFormat
{
    Csv,
    Json,
    Excel
}
