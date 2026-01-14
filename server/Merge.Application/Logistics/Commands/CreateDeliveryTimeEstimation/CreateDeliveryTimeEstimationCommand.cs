using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Commands.CreateDeliveryTimeEstimation;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateDeliveryTimeEstimationCommand(
    Guid? ProductId,
    Guid? CategoryId,
    Guid? WarehouseId,
    Guid? ShippingProviderId,
    string? City,
    string? Country,
    int MinDays,
    int MaxDays,
    int AverageDays,
    bool IsActive,
    DeliveryTimeSettingsDto? Conditions) : IRequest<DeliveryTimeEstimationDto>;

