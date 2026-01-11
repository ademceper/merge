using MediatR;

namespace Merge.Application.Product.Commands.DeleteProductTemplate;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteProductTemplateCommand(
    Guid Id
) : IRequest<bool>;
