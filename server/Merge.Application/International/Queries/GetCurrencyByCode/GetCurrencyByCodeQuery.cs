using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Queries.GetCurrencyByCode;

public record GetCurrencyByCodeQuery(string Code) : IRequest<CurrencyDto?>;

