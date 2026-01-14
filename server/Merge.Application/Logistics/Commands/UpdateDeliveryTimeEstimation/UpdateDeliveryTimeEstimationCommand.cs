using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Commands.UpdateDeliveryTimeEstimation;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record UpdateDeliveryTimeEstimationCommand(
    Guid Id,
    int? MinDays,
    int? MaxDays,
    int? AverageDays,
    bool? IsActive,
    DeliveryTimeSettingsDto? Conditions) : IRequest<Unit>;

