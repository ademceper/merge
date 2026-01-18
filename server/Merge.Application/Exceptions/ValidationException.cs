namespace Merge.Application.Exceptions;

/// <summary>
/// Input validation hatası.
/// HTTP Status: 400 Bad Request
/// </summary>
public class ValidationException : MergeException
{
    /// <summary>
    /// Validation hataları (property name -> error messages)
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(string message)
        : base("VALIDATION_ERROR", message, 400)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(string message, IDictionary<string, string[]> errors)
        : base("VALIDATION_ERROR", message, 400)
    {
        Errors = errors;

        WithMetadata("errors", errors);
    }

    public ValidationException(string propertyName, string errorMessage)
        : this("One or more validation errors occurred", new Dictionary<string, string[]>
        {
            [propertyName] = [errorMessage]
        })
    {
    }

    /// <summary>
    /// FluentValidation ValidationResult'tan ValidationException oluşturur.
    /// </summary>
    public static ValidationException FromFluentValidation(
        FluentValidation.Results.ValidationResult result)
    {
        var errors = result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        return new ValidationException("One or more validation errors occurred", errors);
    }
}

