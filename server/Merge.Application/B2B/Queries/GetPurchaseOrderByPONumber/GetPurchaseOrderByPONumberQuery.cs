using MediatR;
using Merge.Application.DTOs.B2B;

namespace Merge.Application.B2B.Queries.GetPurchaseOrderByPONumber;

public record GetPurchaseOrderByPONumberQuery(string PONumber) : IRequest<PurchaseOrderDto?>;

