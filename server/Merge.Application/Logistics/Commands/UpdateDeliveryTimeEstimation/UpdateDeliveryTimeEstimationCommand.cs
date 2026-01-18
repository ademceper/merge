using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Commands.UpdateDeliveryTimeEstimation;

public record UpdateDeliveryTimeEstimationCommand(
    Guid Id,
    int? MinDays,
    int? MaxDays,
    int? AverageDays,
    bool? IsActive,
    DeliveryTimeSettingsDto? Conditions) : IRequest<Unit>;

