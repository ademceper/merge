using MediatR;
using Merge.Application.DTOs.B2B;

namespace Merge.Application.B2B.Queries.GetPurchaseOrderByPONumber;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetPurchaseOrderByPONumberQuery(string PONumber) : IRequest<PurchaseOrderDto?>;

