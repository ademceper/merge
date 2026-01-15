using MediatR;

namespace Merge.Application.LiveCommerce.Commands.ShowcaseProduct;

public record ShowcaseProductCommand(
    Guid StreamId,
    Guid ProductId) : IRequest<Unit>;
