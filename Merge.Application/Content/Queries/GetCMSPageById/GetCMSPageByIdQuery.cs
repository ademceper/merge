using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Content.Queries.GetCMSPageById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetCMSPageByIdQuery(
    Guid Id
) : IRequest<CMSPageDto?>;

