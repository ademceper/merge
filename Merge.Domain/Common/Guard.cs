namespace Merge.Domain.Common;

/// <summary>
/// Guard clauses for domain validation
/// </summary>
public static class Guard
{
    public static void Against<TException>(bool condition, string message) where TException : Exception
    {
        if (condition)
            throw (TException)Activator.CreateInstance(typeof(TException), message)!;
    }

    public static void AgainstDefault<T>(T value, string parameterName) where T : struct
    {
        if (EqualityComparer<T>.Default.Equals(value, default))
            throw new ArgumentException($"{parameterName} cannot be default value", parameterName);
    }

    public static void AgainstNull<T>(T? value, string parameterName) where T : class
    {
        if (value == null)
            throw new ArgumentNullException(parameterName);
    }

    public static void AgainstNullOrEmpty(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{parameterName} cannot be null or empty", parameterName);
    }

    public static void AgainstNegative(decimal value, string parameterName)
    {
        if (value < 0)
            throw new ArgumentException($"{parameterName} cannot be negative", parameterName);
    }

    public static void AgainstNegativeOrZero(decimal value, string parameterName)
    {
        if (value <= 0)
            throw new ArgumentException($"{parameterName} must be positive", parameterName);
    }

    public static void AgainstOutOfRange<T>(T value, T min, T max, string parameterName) where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
            throw new ArgumentOutOfRangeException(parameterName, $"{parameterName} must be between {min} and {max}");
    }
}

