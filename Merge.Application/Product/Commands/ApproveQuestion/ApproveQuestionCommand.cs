using MediatR;

namespace Merge.Application.Product.Commands.ApproveQuestion;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ApproveQuestionCommand(
    Guid QuestionId
) : IRequest<bool>;
