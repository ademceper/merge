using MediatR;

namespace Merge.Application.Marketing.Commands.DeleteFlashSale;

public record DeleteFlashSaleCommand(
    Guid Id) : IRequest<bool>;
