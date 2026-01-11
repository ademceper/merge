using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Commands.GenerateInvoice;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GenerateInvoiceCommand(
    CreateSellerInvoiceDto Dto
) : IRequest<SellerInvoiceDto>;
