using MediatR;

namespace Merge.Application.Product.Commands.MarkQuestionHelpful;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record MarkQuestionHelpfulCommand(
    Guid UserId,
    Guid QuestionId
) : IRequest;
