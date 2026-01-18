using MediatR;
using Merge.Application.DTOs.Support;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Support.Commands.CreateTicket;

public record CreateTicketCommand(
    Guid UserId,
    string Category,
    string Priority,
    string Subject,
    string Description,
    Guid? OrderId = null,
    Guid? ProductId = null
) : IRequest<SupportTicketDto>;
