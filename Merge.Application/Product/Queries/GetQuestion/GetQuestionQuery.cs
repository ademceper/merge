using MediatR;
using Merge.Application.DTOs.Product;

namespace Merge.Application.Product.Queries.GetQuestion;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetQuestionQuery(
    Guid QuestionId,
    Guid? UserId = null
) : IRequest<ProductQuestionDto?>;
