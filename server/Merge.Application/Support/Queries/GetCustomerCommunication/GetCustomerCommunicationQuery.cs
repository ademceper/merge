using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Queries.GetCustomerCommunication;

public record GetCustomerCommunicationQuery(
    Guid CommunicationId,
    Guid? UserId = null
) : IRequest<CustomerCommunicationDto?>;
