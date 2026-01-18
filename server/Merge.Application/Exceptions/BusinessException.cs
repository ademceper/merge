namespace Merge.Application.Exceptions;

/// <summary>
/// Business rule ihlalleri için exception.
/// HTTP Status: 422 Unprocessable Entity
/// </summary>
public class BusinessException : MergeException
{
    public BusinessException(string message)
        : base("BUSINESS_ERROR", message, 422)
    {
    }

    public BusinessException(string message, Exception innerException)
        : base("BUSINESS_ERROR", message, 422, innerException)
    {
    }

    public BusinessException(string errorCode, string message)
        : base(errorCode, message, 422)
    {
    }
}

