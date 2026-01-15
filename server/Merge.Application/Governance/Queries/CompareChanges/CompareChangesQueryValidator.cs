using FluentValidation;
using Merge.Domain.SharedKernel;

namespace Merge.Application.Governance.Queries.CompareChanges;

public class CompareChangesQueryValidator : AbstractValidator<CompareChangesQuery>
{
    public CompareChangesQueryValidator()
    {
    }
}
