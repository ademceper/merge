using MediatR;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Marketing.Commands.UnsubscribeEmail;

public record UnsubscribeEmailCommand(string Email) : IRequest<bool>;
