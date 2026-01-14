using MediatR;

namespace Merge.Application.Content.Queries.GetSitemapXml;

public record GetSitemapXmlQuery() : IRequest<string>;

