using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Commands.ConvertPrice;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ConvertPriceCommand(
    decimal Amount,
    string FromCurrency,
    string ToCurrency) : IRequest<ConvertedPriceDto>;

