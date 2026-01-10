using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Queries.GetFlashSaleById;

public record GetFlashSaleByIdQuery(
    Guid Id) : IRequest<FlashSaleDto?>;
