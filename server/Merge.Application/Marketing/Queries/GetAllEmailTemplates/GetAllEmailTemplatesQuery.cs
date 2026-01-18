using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Queries.GetAllEmailTemplates;

public record GetAllEmailTemplatesQuery(string? Type, int PageNumber, int PageSize) : IRequest<PagedResult<EmailTemplateDto>>;
