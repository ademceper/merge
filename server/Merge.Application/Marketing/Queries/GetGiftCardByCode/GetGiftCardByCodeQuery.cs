using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Queries.GetGiftCardByCode;

public record GetGiftCardByCodeQuery(
    string Code) : IRequest<GiftCardDto?>;
