using FluentValidation;

namespace Merge.Application.Analytics.Queries.GetCustomerLifetimeValue;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetCustomerLifetimeValueQueryValidator : AbstractValidator<GetCustomerLifetimeValueQuery>
{
    public GetCustomerLifetimeValueQueryValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Müşteri ID zorunludur");
    }
}

