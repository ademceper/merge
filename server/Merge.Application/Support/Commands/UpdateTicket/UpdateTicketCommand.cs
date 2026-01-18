using MediatR;
using Merge.Application.DTOs.Support;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Support.Commands.UpdateTicket;

public record UpdateTicketCommand(
    Guid TicketId,
    string? Subject = null,
    string? Description = null,
    string? Category = null,
    string? Priority = null,
    string? Status = null,
    Guid? AssignedToId = null
) : IRequest<bool>;
