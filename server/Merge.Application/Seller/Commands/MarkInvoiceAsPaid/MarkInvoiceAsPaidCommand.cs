using MediatR;

namespace Merge.Application.Seller.Commands.MarkInvoiceAsPaid;

public record MarkInvoiceAsPaidCommand(
    Guid InvoiceId
) : IRequest<bool>;
