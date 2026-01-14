using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Content.Queries.GetHomePageCMSPage;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetHomePageCMSPageQuery() : IRequest<CMSPageDto?>;

