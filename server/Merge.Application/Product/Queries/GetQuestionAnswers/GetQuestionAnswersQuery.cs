using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetQuestionAnswers;

public record GetQuestionAnswersQuery(
    Guid QuestionId,
    Guid? UserId = null
) : IRequest<IEnumerable<ProductAnswerDto>>;
