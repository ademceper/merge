using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Commands.GenerateInvoice;

public record GenerateInvoiceCommand(
    CreateSellerInvoiceDto Dto
) : IRequest<SellerInvoiceDto>;
