using MediatR;

namespace Merge.Domain.SharedKernel;


public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}

