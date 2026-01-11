using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Commands.SubmitSellerApplication;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record SubmitSellerApplicationCommand(
    Guid UserId,
    CreateSellerApplicationDto ApplicationDto
) : IRequest<SellerApplicationDto>;
