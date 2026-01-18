using MediatR;
using Merge.Application.DTOs.Order;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Commands.ExportOrders;

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
