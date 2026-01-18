using MediatR;
using Merge.Application.DTOs.Seller;
using Merge.Domain.Enums;

namespace Merge.Application.Seller.Commands.ReviewSellerApplication;

public record ReviewSellerApplicationCommand(
    Guid ApplicationId,
    SellerApplicationStatus Status,
    string? RejectionReason = null,
    string? AdditionalNotes = null,
    Guid ReviewerId = default
) : IRequest<SellerApplicationDto>;
