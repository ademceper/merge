using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Content.Queries.GetCMSPageById;

public record GetCMSPageByIdQuery(
    Guid Id
) : IRequest<CMSPageDto?>;

