using MediatR;
using Merge.Application.DTOs.Security;
using Merge.Domain.Enums;

namespace Merge.Application.Security.Commands.CreateOrderVerification;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateOrderVerificationCommand(
    Guid OrderId,
    string VerificationType,
    string? VerificationMethod = null,
    string? VerificationNotes = null,
    bool RequiresManualReview = false
) : IRequest<OrderVerificationDto>;
