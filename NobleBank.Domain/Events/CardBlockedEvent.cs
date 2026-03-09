using MediatR;

namespace NobleBank.Domain.Events
{
    public record CardBlockedEvent(
        Guid CardId,
        string UserId,
        string Reason,
        string PerformedBy,
        DateTime OccurredAt) : INotification;
}
