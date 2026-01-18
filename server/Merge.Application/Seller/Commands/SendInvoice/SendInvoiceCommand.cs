using MediatR;

namespace Merge.Application.Seller.Commands.SendInvoice;

public record SendInvoiceCommand(
    Guid InvoiceId
) : IRequest<bool>;
