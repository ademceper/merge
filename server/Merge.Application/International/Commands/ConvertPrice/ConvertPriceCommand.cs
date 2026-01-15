using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Commands.ConvertPrice;

public record ConvertPriceCommand(
    decimal Amount,
    string FromCurrency,
    string ToCurrency) : IRequest<ConvertedPriceDto>;

