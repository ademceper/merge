using FluentValidation;

namespace Merge.Application.Analytics.Queries.GetSystemHealth;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetSystemHealthQueryValidator : AbstractValidator<GetSystemHealthQuery>
{
    public GetSystemHealthQueryValidator()
    {
        // GetSystemHealthQuery parametre almadığı için validation gerekmez
        // Ancak FluentValidation pattern'i için boş validator oluşturuyoruz
    }
}

