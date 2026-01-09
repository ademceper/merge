using MediatR;

namespace Merge.Application.Catalog.Commands.DeleteCategory;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteCategoryCommand(
    Guid Id
) : IRequest<bool>;
