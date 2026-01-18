using MediatR;
using Merge.Application.DTOs.B2B;

namespace Merge.Application.B2B.Commands.CreateWholesalePrice;

public record CreateWholesalePriceCommand(CreateWholesalePriceDto Dto) : IRequest<WholesalePriceDto>;

