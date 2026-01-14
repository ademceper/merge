using MediatR;
using Merge.Application.DTOs.User;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.User.Queries.GetMostViewedProducts;

public record GetMostViewedProductsQuery(
    int Days = 30,
    int TopN = 10
) : IRequest<List<PopularProductDto>>;
