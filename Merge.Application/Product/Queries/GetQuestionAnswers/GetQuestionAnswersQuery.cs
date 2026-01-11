using MediatR;
using Merge.Application.DTOs.Product;

namespace Merge.Application.Product.Queries.GetQuestionAnswers;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetQuestionAnswersQuery(
    Guid QuestionId,
    Guid? UserId = null
) : IRequest<IEnumerable<ProductAnswerDto>>;
