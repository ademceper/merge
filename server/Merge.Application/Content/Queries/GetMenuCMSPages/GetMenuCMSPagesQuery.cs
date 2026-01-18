using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Content.Queries.GetMenuCMSPages;

public record GetMenuCMSPagesQuery() : IRequest<IEnumerable<CMSPageDto>>;

