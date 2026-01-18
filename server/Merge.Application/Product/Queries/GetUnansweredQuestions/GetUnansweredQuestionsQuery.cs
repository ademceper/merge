using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetUnansweredQuestions;

public record GetUnansweredQuestionsQuery(
    Guid? ProductId = null,
    int Limit = 20
) : IRequest<IEnumerable<ProductQuestionDto>>;
