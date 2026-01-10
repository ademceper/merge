using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Queries.GetEmailSubscriberByEmail;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetEmailSubscriberByEmailQuery(string Email) : IRequest<EmailSubscriberDto?>;
