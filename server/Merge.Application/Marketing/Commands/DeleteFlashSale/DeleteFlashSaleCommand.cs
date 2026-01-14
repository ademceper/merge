using MediatR;

namespace Merge.Application.Marketing.Commands.DeleteFlashSale;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteFlashSaleCommand(
    Guid Id) : IRequest<bool>;
