using MediatR;
using Merge.Application.DTOs.Order;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Commands.CreateReturnRequest;

public record CreateReturnRequestCommand(
    CreateReturnRequestDto Dto
) : IRequest<ReturnRequestDto>;
