using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Content.Queries.GetHomePageCMSPage;

public record GetHomePageCMSPageQuery() : IRequest<CMSPageDto?>;

