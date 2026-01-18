using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Queries.GetReturnRequestById;

public class GetReturnRequestByIdQueryValidator : AbstractValidator<GetReturnRequestByIdQuery>
{
    public GetReturnRequestByIdQueryValidator()
    {
        RuleFor(x => x.ReturnRequestId)
            .NotEmpty()
            .WithMessage("İade talebi ID'si zorunludur.");
    }
}
