using MediatR;

namespace Merge.Application.Product.Commands.DeleteQuestion;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteQuestionCommand(
    Guid QuestionId,
    Guid UserId
) : IRequest<bool>;
