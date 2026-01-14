using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Content.Queries.GetAllPageBuilders;

public record GetAllPageBuildersQuery(
    string? Status = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<PageBuilderDto>>;

