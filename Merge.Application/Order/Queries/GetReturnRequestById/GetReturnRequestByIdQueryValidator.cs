using FluentValidation;

namespace Merge.Application.Order.Queries.GetReturnRequestById;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetReturnRequestByIdQueryValidator : AbstractValidator<GetReturnRequestByIdQuery>
{
    public GetReturnRequestByIdQueryValidator()
    {
        RuleFor(x => x.ReturnRequestId)
            .NotEmpty()
            .WithMessage("İade talebi ID'si zorunludur.");
    }
}
