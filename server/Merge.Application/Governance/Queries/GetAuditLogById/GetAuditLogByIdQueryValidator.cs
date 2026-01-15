using FluentValidation;
using Merge.Domain.SharedKernel;

namespace Merge.Application.Governance.Queries.GetAuditLogById;

public class GetAuditLogByIdQueryValidator : AbstractValidator<GetAuditLogByIdQuery>
{
    public GetAuditLogByIdQueryValidator()
    {
    }
}
