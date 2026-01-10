using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Queries.GetEmailSubscriberById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetEmailSubscriberByIdQuery(Guid Id) : IRequest<EmailSubscriberDto?>;
