using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Application.Common;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetUserQuestions;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 3.4: Pagination (ZORUNLU)
public record GetUserQuestionsQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<ProductQuestionDto>>;
