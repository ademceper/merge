using MediatR;
using Merge.Application.DTOs.Seller;
using Merge.Domain.Enums;

namespace Merge.Application.Seller.Commands.ReviewSellerApplication;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ReviewSellerApplicationCommand(
    Guid ApplicationId,
    SellerApplicationStatus Status,
    string? RejectionReason = null,
    string? AdditionalNotes = null,
    Guid ReviewerId = default
) : IRequest<SellerApplicationDto>;
