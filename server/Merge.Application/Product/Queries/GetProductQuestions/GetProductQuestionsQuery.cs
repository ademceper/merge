using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Application.Common;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetProductQuestions;

public record GetProductQuestionsQuery(
    Guid ProductId,
    Guid? UserId = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<ProductQuestionDto>>;
