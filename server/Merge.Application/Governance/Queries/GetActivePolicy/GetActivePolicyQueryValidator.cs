using FluentValidation;
using Merge.Domain.Modules.Content;

namespace Merge.Application.Governance.Queries.GetActivePolicy;

public class GetActivePolicyQueryValidator : AbstractValidator<GetActivePolicyQuery>
{
    public GetActivePolicyQueryValidator()
    {
    }
}

