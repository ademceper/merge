using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Queries.GetCustomerCommunication;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 3.2: IDOR koruması - userId parametresi eklendi
public record GetCustomerCommunicationQuery(
    Guid CommunicationId,
    Guid? UserId = null
) : IRequest<CustomerCommunicationDto?>;
