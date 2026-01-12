using MediatR;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.User.Commands.DeleteOldActivities;

public record DeleteOldActivitiesCommand(int DaysToKeep = 90) : IRequest;
