using MediatR;
using Merge.Application.DTOs.Security;

namespace Merge.Application.Security.Commands.CreatePaymentFraudCheck;

public record CreatePaymentFraudCheckCommand(
    Guid PaymentId,
    string CheckType,
    string? DeviceFingerprint = null,
    string? IpAddress = null,
    string? UserAgent = null
) : IRequest<PaymentFraudPreventionDto>;
