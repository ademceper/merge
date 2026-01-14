namespace Merge.Domain.Exceptions;

/// <summary>
/// Domain layer exception for business rule violations
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception for invalid state transitions
/// </summary>
public class InvalidStateTransitionException : DomainException
{
    public object CurrentState { get; }
    public object AttemptedState { get; }

    public InvalidStateTransitionException(object currentState, object attemptedState)
        : base($"Invalid state transition from {currentState} to {attemptedState}")
    {
        CurrentState = currentState;
        AttemptedState = attemptedState;
    }
}

