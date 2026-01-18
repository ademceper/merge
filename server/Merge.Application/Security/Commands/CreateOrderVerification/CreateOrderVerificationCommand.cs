using MediatR;
using Merge.Application.DTOs.Security;
using Merge.Domain.Enums;

namespace Merge.Application.Security.Commands.CreateOrderVerification;

public record CreateOrderVerificationCommand(
    Guid OrderId,
    string VerificationType,
    string? VerificationMethod = null,
    string? VerificationNotes = null,
    bool RequiresManualReview = false
) : IRequest<OrderVerificationDto>;
