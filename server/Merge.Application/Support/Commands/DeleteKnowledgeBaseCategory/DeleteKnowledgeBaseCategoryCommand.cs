using MediatR;

namespace Merge.Application.Support.Commands.DeleteKnowledgeBaseCategory;

public record DeleteKnowledgeBaseCategoryCommand(
    Guid CategoryId
) : IRequest<bool>;
