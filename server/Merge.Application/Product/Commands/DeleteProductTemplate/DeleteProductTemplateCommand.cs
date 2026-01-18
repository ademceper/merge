using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.DeleteProductTemplate;

public record DeleteProductTemplateCommand(
    Guid Id
) : IRequest<bool>;
