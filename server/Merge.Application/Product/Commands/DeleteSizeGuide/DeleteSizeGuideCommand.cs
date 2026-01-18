using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.DeleteSizeGuide;

public record DeleteSizeGuideCommand(
    Guid Id
) : IRequest<bool>;
