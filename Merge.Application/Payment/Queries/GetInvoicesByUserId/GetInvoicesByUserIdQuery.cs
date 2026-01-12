using MediatR;
using Merge.Application.DTOs.Payment;
using Merge.Application.Common;
using Merge.Domain.Modules.Payment;

namespace Merge.Application.Payment.Queries.GetInvoicesByUserId;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// BOLUM 3.4: Pagination (ZORUNLU)
public record GetInvoicesByUserIdQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<InvoiceDto>>;
