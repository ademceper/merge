using MediatR;

namespace Merge.Application.Content.Queries.GetRobotsTxt;

public record GetRobotsTxtQuery() : IRequest<string>;

