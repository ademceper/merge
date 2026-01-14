using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.DeleteProductTemplate;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteProductTemplateCommand(
    Guid Id
) : IRequest<bool>;
