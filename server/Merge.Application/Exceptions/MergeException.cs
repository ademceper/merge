namespace Merge.Application.Exceptions;

/// <summary>
/// Tüm custom exception'lar için base class.
/// Error code, HTTP status code ve metadata desteği sağlar.
/// </summary>
public abstract class MergeException : Exception
{
    /// <summary>
    /// Error code (örn: "RESOURCE_NOT_FOUND", "VALIDATION_ERROR")
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// HTTP status code (örn: 400, 404, 422)
    /// </summary>
    public int HttpStatusCode { get; }

    /// <summary>
    /// Ek metadata bilgileri
    /// </summary>
    public IDictionary<string, object> Metadata { get; }

    protected MergeException(
        string errorCode,
        string message,
        int httpStatusCode = 500,
        Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        HttpStatusCode = httpStatusCode;
        Metadata = new Dictionary<string, object>();
    }

    /// <summary>
    /// Metadata ekler.
    /// </summary>
    public MergeException WithMetadata(string key, object value)
    {
        Metadata[key] = value;
        return this;
    }
}
