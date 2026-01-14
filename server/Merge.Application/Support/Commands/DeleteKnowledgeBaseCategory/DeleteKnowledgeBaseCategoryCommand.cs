using MediatR;

namespace Merge.Application.Support.Commands.DeleteKnowledgeBaseCategory;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteKnowledgeBaseCategoryCommand(
    Guid CategoryId
) : IRequest<bool>;
