using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Queries.GetCouponByCode;

public record GetCouponByCodeQuery(
    string Code
) : IRequest<CouponDto?>;
