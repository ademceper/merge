using FluentValidation;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.Governance.Queries.GetUserAcceptances;

public class GetUserAcceptancesQueryValidator() : AbstractValidator<GetUserAcceptancesQuery>
{
    public GetUserAcceptancesQueryValidator()
    {
    }
}

