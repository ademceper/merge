using MediatR;
using Merge.Application.DTOs.Product;

namespace Merge.Application.Product.Queries.GetUnansweredQuestions;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetUnansweredQuestionsQuery(
    Guid? ProductId = null,
    int Limit = 20
) : IRequest<IEnumerable<ProductQuestionDto>>;
