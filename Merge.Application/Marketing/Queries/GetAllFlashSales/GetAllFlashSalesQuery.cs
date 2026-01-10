using MediatR;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;

namespace Merge.Application.Marketing.Queries.GetAllFlashSales;

public record GetAllFlashSalesQuery(
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<FlashSaleDto>>;
