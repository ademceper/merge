using FluentValidation;
using Merge.Domain.Modules.Content;

namespace Merge.Application.Governance.Queries.GetAcceptanceCount;

public class GetAcceptanceCountQueryValidator : AbstractValidator<GetAcceptanceCountQuery>
{
    public GetAcceptanceCountQueryValidator()
    {
    }
}

