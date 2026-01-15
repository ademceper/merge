using FluentValidation;
using Merge.Domain.Modules.Content;

namespace Merge.Application.Governance.Queries.GetPolicyById;

public class GetPolicyByIdQueryValidator() : AbstractValidator<GetPolicyByIdQuery>
{
    public GetPolicyByIdQueryValidator()
    {
    }
}

