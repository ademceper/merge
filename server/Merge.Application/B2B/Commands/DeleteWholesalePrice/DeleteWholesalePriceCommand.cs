using MediatR;

namespace Merge.Application.B2B.Commands.DeleteWholesalePrice;

public record DeleteWholesalePriceCommand(Guid Id) : IRequest<bool>;

