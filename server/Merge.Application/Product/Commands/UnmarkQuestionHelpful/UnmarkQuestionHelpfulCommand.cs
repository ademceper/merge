using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.UnmarkQuestionHelpful;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record UnmarkQuestionHelpfulCommand(
    Guid UserId,
    Guid QuestionId
) : IRequest;
