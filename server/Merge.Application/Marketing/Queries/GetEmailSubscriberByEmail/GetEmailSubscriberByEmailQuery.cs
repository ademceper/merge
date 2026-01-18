using MediatR;
using Merge.Application.DTOs.Marketing;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Marketing.Queries.GetEmailSubscriberByEmail;

public record GetEmailSubscriberByEmailQuery(string Email) : IRequest<EmailSubscriberDto?>;
