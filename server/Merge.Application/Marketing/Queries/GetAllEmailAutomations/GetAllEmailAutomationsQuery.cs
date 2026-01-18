using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Queries.GetAllEmailAutomations;

public record GetAllEmailAutomationsQuery(int PageNumber, int PageSize) : IRequest<PagedResult<EmailAutomationDto>>;
