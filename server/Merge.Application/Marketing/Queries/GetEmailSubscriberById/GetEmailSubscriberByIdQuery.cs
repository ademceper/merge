using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Queries.GetEmailSubscriberById;

public record GetEmailSubscriberByIdQuery(Guid Id) : IRequest<EmailSubscriberDto?>;
