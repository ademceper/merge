using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Queries.GetGiftCardById;

public record GetGiftCardByIdQuery(
    Guid Id) : IRequest<GiftCardDto?>;
