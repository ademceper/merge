using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Queries.GetCouponById;

public record GetCouponByIdQuery(
    Guid Id
) : IRequest<CouponDto?>;
