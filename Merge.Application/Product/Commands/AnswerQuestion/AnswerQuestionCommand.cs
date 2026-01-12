using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.AnswerQuestion;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record AnswerQuestionCommand(
    Guid UserId,
    Guid QuestionId,
    string Answer
) : IRequest<ProductAnswerDto>;
