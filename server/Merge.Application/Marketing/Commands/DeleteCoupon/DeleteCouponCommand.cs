using MediatR;

namespace Merge.Application.Marketing.Commands.DeleteCoupon;

public record DeleteCouponCommand(
    Guid Id) : IRequest<bool>;
