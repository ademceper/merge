using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Queries.GetCurrencyById;

public record GetCurrencyByIdQuery(Guid Id) : IRequest<CurrencyDto?>;

