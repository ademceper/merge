using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Content.Queries.GetMenuCMSPages;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetMenuCMSPagesQuery() : IRequest<IEnumerable<CMSPageDto>>;

