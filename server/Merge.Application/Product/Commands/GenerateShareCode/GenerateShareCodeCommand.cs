using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.GenerateShareCode;

public record GenerateShareCodeCommand(
    Guid ComparisonId
) : IRequest<string>;
