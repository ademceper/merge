using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Commands.SubmitSellerApplication;

public record SubmitSellerApplicationCommand(
    Guid UserId,
    CreateSellerApplicationDto ApplicationDto
) : IRequest<SellerApplicationDto>;
