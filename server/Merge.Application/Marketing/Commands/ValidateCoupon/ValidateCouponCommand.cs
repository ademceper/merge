using MediatR;

namespace Merge.Application.Marketing.Commands.ValidateCoupon;

public record ValidateCouponCommand(
    string Code,
    decimal OrderAmount,
    Guid? UserId,
    List<Guid>? ProductIds) : IRequest<decimal>;
